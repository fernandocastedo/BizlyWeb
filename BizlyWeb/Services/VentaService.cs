using BizlyWeb.Models.DTOs;
using Microsoft.AspNetCore.Http;

namespace BizlyWeb.Services
{
    /// <summary>
    /// Servicio de negocio para administración de ventas.
    /// Capa de Negocio
    /// </summary>
    public class VentaService
    {
        private readonly ApiService _apiService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ProductoService _productoService;
        private readonly InventarioService _inventarioService;

        public VentaService(
            ApiService apiService,
            IHttpContextAccessor httpContextAccessor,
            ProductoService productoService,
            InventarioService inventarioService)
        {
            _apiService = apiService;
            _httpContextAccessor = httpContextAccessor;
            _productoService = productoService;
            _inventarioService = inventarioService;
        }

        private string? GetEmpresaId()
        {
            return SessionHelper.GetEmpresaId(_httpContextAccessor);
        }

        private string? GetUsuarioId()
        {
            return SessionHelper.GetUsuarioId(_httpContextAccessor);
        }

        /// <summary>
        /// Obtiene todas las ventas de la empresa actual
        /// </summary>
        public async Task<List<VentaDto>> ObtenerVentasAsync()
        {
            var empresaId = GetEmpresaId();
            if (string.IsNullOrEmpty(empresaId))
            {
                return new List<VentaDto>();
            }

            var data = await _apiService.GetAsync<List<VentaDto>>("/api/ventas") ?? new List<VentaDto>();
            return data.Where(v => v.EmpresaId == empresaId).ToList();
        }

        /// <summary>
        /// Obtiene una venta por ID
        /// </summary>
        public async Task<VentaDto?> ObtenerVentaPorIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            return await _apiService.GetAsync<VentaDto>($"/api/ventas/{id}");
        }

        /// <summary>
        /// Obtiene los detalles de una venta
        /// </summary>
        public async Task<List<DetalleVentaDto>> ObtenerDetallesVentaAsync(string ventaId)
        {
            if (string.IsNullOrEmpty(ventaId))
            {
                return new List<DetalleVentaDto>();
            }

            var data = await _apiService.GetAsync<List<DetalleVentaDto>>($"/api/detalleventas/venta/{ventaId}") ?? new List<DetalleVentaDto>();
            return data;
        }

        /// <summary>
        /// Crea una nueva venta con validación de stock y descuento automático (RF-22, RF-23, RF-24, RF-25, RF-21)
        /// </summary>
        public async Task<VentaDto?> CrearVentaAsync(
            string? clienteId,
            string metodoPago,
            bool esEnvio,
            List<DetalleVentaDto> detalles)
        {
            var empresaId = GetEmpresaId();
            var usuarioId = GetUsuarioId();
            if (string.IsNullOrEmpty(empresaId) || string.IsNullOrEmpty(usuarioId))
            {
                throw new InvalidOperationException("No se pudo obtener la empresa o usuario actual.");
            }

            // Validar stock disponible para todos los productos (RF-23)
            foreach (var detalle in detalles)
            {
                var (stockSuficiente, insumosInsuficientes) = await _productoService.ValidarStockDisponibleAsync(
                    detalle.ProductoVentaId,
                    detalle.Cantidad
                );

                if (!stockSuficiente)
                {
                    throw new InvalidOperationException(
                        $"Stock insuficiente para el producto. {string.Join(", ", insumosInsuficientes)}"
                    );
                }
            }

            // Obtener sucursal (primera disponible)
            var sucursalService = new SucursalService(_apiService, _httpContextAccessor);
            var sucursales = await sucursalService.ObtenerSucursalesAsync();
            if (!sucursales.Any())
            {
                throw new InvalidOperationException("No hay sucursales disponibles.");
            }

            // Calcular total (RF-24)
            decimal total = detalles.Sum(d => d.Subtotal);

            // Crear la venta
            var venta = new VentaDto
            {
                EmpresaId = empresaId,
                SucursalId = sucursales.First().Id!,
                UsuarioId = usuarioId,
                ClienteId = clienteId,
                Fecha = DateTime.UtcNow,
                MetodoPago = metodoPago,
                Total = total,
                EsEnvio = esEnvio,
                EstadoPago = "pagado",
                EstadoPedido = esEnvio ? "pendiente" : "completado"
            };

            var ventaCreada = await _apiService.PostAsync<VentaDto, VentaDto>("/api/ventas", venta);
            if (ventaCreada == null || string.IsNullOrEmpty(ventaCreada.Id))
            {
                throw new InvalidOperationException("No se pudo crear la venta.");
            }

            // Crear detalles de venta
            foreach (var detalle in detalles)
            {
                detalle.VentaId = ventaCreada.Id;
                await _apiService.PostAsync<DetalleVentaDto, DetalleVentaDto>("/api/detalleventas", detalle);
            }

            // Descontar inventario automáticamente (RF-21)
            foreach (var detalle in detalles)
            {
                await _productoService.DescontarInventarioPorVentaAsync(
                    detalle.ProductoVentaId,
                    detalle.Cantidad,
                    $"Venta #{ventaCreada.Id}"
                );
            }

            return ventaCreada;
        }

        /// <summary>
        /// Actualiza el estado de una venta (RF-31)
        /// </summary>
        public async Task<bool> ActualizarEstadoVentaAsync(string ventaId, string estadoPedido, string? estadoPago = null)
        {
            if (string.IsNullOrEmpty(ventaId))
            {
                return false;
            }

            var venta = await ObtenerVentaPorIdAsync(ventaId);
            if (venta == null)
            {
                return false;
            }

            venta.EstadoPedido = estadoPedido;
            if (!string.IsNullOrEmpty(estadoPago))
            {
                venta.EstadoPago = estadoPago;
            }

            var response = await _apiService.PutAsync<VentaDto, VentaDto>($"/api/ventas/{ventaId}", venta);
            return response != null;
        }

        /// <summary>
        /// Cancela una venta y devuelve el stock al inventario (RF-28)
        /// </summary>
        public async Task<bool> CancelarVentaAsync(string ventaId, string motivo = "Cancelación de venta")
        {
            if (string.IsNullOrEmpty(ventaId))
            {
                return false;
            }

            var venta = await ObtenerVentaPorIdAsync(ventaId);
            if (venta == null)
            {
                return false;
            }

            // Si ya está cancelada, no hacer nada
            if (venta.EstadoPedido == "cancelado")
            {
                return true;
            }

            // Obtener detalles de la venta
            var detalles = await ObtenerDetallesVentaAsync(ventaId);

            // Devolver stock al inventario
            foreach (var detalle in detalles)
            {
                // Obtener insumos del producto
                var insumosProducto = await _productoService.ObtenerInsumosDelProductoAsync(detalle.ProductoVentaId);
                var todosInsumos = await _inventarioService.ObtenerInsumosAsync();
                var insumosDict = todosInsumos.ToDictionary(i => i.Id!, i => i);

                foreach (var insumoProducto in insumosProducto)
                {
                    if (insumosDict.ContainsKey(insumoProducto.InsumoId))
                    {
                        var insumo = insumosDict[insumoProducto.InsumoId];
                        var cantidadADevolver = insumoProducto.CantidadUtilizada * detalle.Cantidad;
                        var nuevaCantidad = insumo.Cantidad + cantidadADevolver;

                        await _inventarioService.ActualizarStockAsync(
                            insumo.Id!,
                            nuevaCantidad,
                            "entrada",
                            $"{motivo} - Venta #{ventaId}"
                        );
                    }
                }
            }

            // Actualizar estado de la venta
            venta.EstadoPedido = "cancelado";
            var response = await _apiService.PutAsync<VentaDto, VentaDto>($"/api/ventas/{ventaId}", venta);
            return response != null;
        }

        /// <summary>
        /// Obtiene las ventas con filtros (RF-26)
        /// </summary>
        public async Task<List<VentaDto>> ObtenerVentasFiltradasAsync(
            DateTime? fechaInicio = null,
            DateTime? fechaFin = null,
            string? estadoPedido = null,
            string? estadoPago = null)
        {
            var ventas = await ObtenerVentasAsync();

            if (fechaInicio.HasValue)
            {
                ventas = ventas.Where(v => v.Fecha >= fechaInicio.Value).ToList();
            }

            if (fechaFin.HasValue)
            {
                ventas = ventas.Where(v => v.Fecha <= fechaFin.Value.AddDays(1).AddSeconds(-1)).ToList();
            }

            if (!string.IsNullOrEmpty(estadoPedido))
            {
                ventas = ventas.Where(v => v.EstadoPedido == estadoPedido).ToList();
            }

            if (!string.IsNullOrEmpty(estadoPago))
            {
                ventas = ventas.Where(v => v.EstadoPago == estadoPago).ToList();
            }

            return ventas.OrderByDescending(v => v.Fecha).ToList();
        }

        /// <summary>
        /// Obtiene pedidos pendientes (RF-30)
        /// </summary>
        public async Task<List<VentaDto>> ObtenerPedidosPendientesAsync()
        {
            return await ObtenerVentasFiltradasAsync(estadoPedido: "pendiente");
        }

        /// <summary>
        /// Obtiene el top de vendedores (RF-27)
        /// </summary>
        public async Task<List<(string UsuarioId, string UsuarioNombre, int TotalVentas, decimal TotalIngresos)>> ObtenerTopVendedoresAsync(DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            var ventas = await ObtenerVentasFiltradasAsync(fechaInicio, fechaFin, estadoPedido: "completado");

            var topVendedores = ventas
                .Where(v => v.EstadoPedido == "completado")
                .GroupBy(v => v.UsuarioId)
                .Select(g => new
                {
                    UsuarioId = g.Key,
                    TotalVentas = g.Count(),
                    TotalIngresos = g.Sum(v => v.Total)
                })
                .OrderByDescending(x => x.TotalIngresos)
                .Take(10)
                .ToList();

            // TODO: Obtener nombres de usuarios desde la API
            return topVendedores.Select(tv => (
                tv.UsuarioId,
                UsuarioNombre: $"Usuario {tv.UsuarioId.Substring(0, Math.Min(8, tv.UsuarioId.Length))}",
                tv.TotalVentas,
                tv.TotalIngresos
            )).ToList();
        }
    }
}

