using BizlyWeb.Models.DTOs;
using BizlyWeb.Services.Exceptions;
using Microsoft.AspNetCore.Http;

namespace BizlyWeb.Services
{
    /// <summary>
    /// Servicio de negocio para administración de productos de venta.
    /// Capa de Negocio
    /// </summary>
    public class ProductoService
    {
        private readonly ApiService _apiService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly InventarioService _inventarioService;
        private readonly EmpresaService _empresaService;

        public ProductoService(
            ApiService apiService,
            IHttpContextAccessor httpContextAccessor,
            InventarioService inventarioService,
            EmpresaService empresaService)
        {
            _apiService = apiService;
            _httpContextAccessor = httpContextAccessor;
            _inventarioService = inventarioService;
            _empresaService = empresaService;
        }

        private string? GetEmpresaId()
        {
            return SessionHelper.GetEmpresaId(_httpContextAccessor);
        }

        /// <summary>
        /// Obtiene todos los productos de la empresa actual
        /// </summary>
        public async Task<List<ProductoVentaDto>> ObtenerProductosAsync()
        {
            var empresaId = GetEmpresaId();
            if (string.IsNullOrEmpty(empresaId))
            {
                return new List<ProductoVentaDto>();
            }

            try
            {
                var data = await _apiService.GetAsync<List<ProductoVentaDto>>("/api/productosventa") ?? new List<ProductoVentaDto>();
                return data.Where(p => p.EmpresaId == empresaId).ToList();
            }
            catch (ApiException apiEx) when (apiEx.StatusCode == System.Net.HttpStatusCode.Forbidden || apiEx.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                // Re-lanzar ApiException para que el controlador pueda manejarla
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener productos: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene un producto por ID
        /// </summary>
        public async Task<ProductoVentaDto?> ObtenerProductoPorIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            return await _apiService.GetAsync<ProductoVentaDto>($"/api/productosventa/{id}");
        }

        /// <summary>
        /// Crea un nuevo producto
        /// </summary>
        public async Task<ProductoVentaDto?> CrearProductoAsync(ProductoVentaDto producto)
        {
            var empresaId = GetEmpresaId();
            if (string.IsNullOrEmpty(empresaId))
            {
                throw new InvalidOperationException("No se pudo obtener la empresa actual.");
            }

            producto.EmpresaId = empresaId;

            // Si no se especifica sucursal, obtener la primera disponible
            if (string.IsNullOrEmpty(producto.SucursalId))
            {
                var sucursalService = new SucursalService(_apiService, _httpContextAccessor);
                var sucursales = await sucursalService.ObtenerSucursalesAsync();
                if (sucursales.Any())
                {
                    producto.SucursalId = sucursales.First().Id!;
                }
                else
                {
                    throw new InvalidOperationException("No hay sucursales disponibles. Crea una sucursal primero.");
                }
            }

            return await _apiService.PostAsync<ProductoVentaDto, ProductoVentaDto>("/api/productosventa", producto);
        }

        /// <summary>
        /// Actualiza un producto existente
        /// </summary>
        public async Task<bool> ActualizarProductoAsync(ProductoVentaDto producto)
        {
            if (producto == null || string.IsNullOrEmpty(producto.Id))
            {
                return false;
            }

            try
            {
                // La API puede devolver 204 NoContent en actualizaciones exitosas,
                // lo que hace que PutAsync retorne null. Si no hay excepción, la operación fue exitosa.
                await _apiService.PutAsync<ProductoVentaDto, ProductoVentaDto>($"/api/productosventa/{producto.Id}", producto);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Elimina un producto (eliminación lógica: Activo = false)
        /// </summary>
        public async Task<bool> EliminarProductoAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return false;
            }

            var producto = await ObtenerProductoPorIdAsync(id);
            if (producto == null)
            {
                return false;
            }

            producto.Activo = false;
            return await ActualizarProductoAsync(producto);
        }

        /// <summary>
        /// Obtiene los insumos asociados a un producto
        /// </summary>
        public async Task<List<InsumoProductoVentaDto>> ObtenerInsumosDelProductoAsync(string productoId)
        {
            if (string.IsNullOrEmpty(productoId))
            {
                return new List<InsumoProductoVentaDto>();
            }

            var data = await _apiService.GetAsync<List<InsumoProductoVentaDto>>($"/api/insumoproductoventa/producto/{productoId}") ?? new List<InsumoProductoVentaDto>();
            return data;
        }

        /// <summary>
        /// Asocia un insumo a un producto
        /// </summary>
        public async Task<InsumoProductoVentaDto?> AsociarInsumoAsync(string productoId, string insumoId, decimal cantidadUtilizada)
        {
            if (string.IsNullOrEmpty(productoId) || string.IsNullOrEmpty(insumoId))
            {
                return null;
            }

            var dto = new InsumoProductoVentaDto
            {
                ProductoVentaId = productoId,
                InsumoId = insumoId,
                CantidadUtilizada = cantidadUtilizada
            };

            return await _apiService.PostAsync<InsumoProductoVentaDto, InsumoProductoVentaDto>("/api/insumoproductoventa", dto);
        }

        /// <summary>
        /// Actualiza la cantidad de un insumo asociado a un producto
        /// </summary>
        public async Task<bool> ActualizarInsumoProductoAsync(string id, InsumoProductoVentaDto dto)
        {
            if (string.IsNullOrEmpty(id))
            {
                return false;
            }

            var response = await _apiService.PutAsync<InsumoProductoVentaDto, InsumoProductoVentaDto>($"/api/insumoproductoventa/{id}", dto);
            return response != null;
        }

        /// <summary>
        /// Elimina la asociación de un insumo con un producto
        /// </summary>
        public async Task<bool> EliminarInsumoProductoAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return false;
            }

            return await _apiService.DeleteAsync($"/api/insumoproductoventa/{id}");
        }

        /// <summary>
        /// Calcula el precio sugerido basado en el costo de insumos y el margen de ganancia
        /// </summary>
        public async Task<decimal> CalcularPrecioSugeridoAsync(string productoId)
        {
            var insumosProducto = await ObtenerInsumosDelProductoAsync(productoId);
            if (!insumosProducto.Any())
            {
                return 0;
            }

            // Obtener todos los insumos para obtener sus precios
            var todosInsumos = await _inventarioService.ObtenerInsumosAsync();
            var insumosDict = todosInsumos.ToDictionary(i => i.Id!, i => i);

            // Calcular costo total
            decimal costoTotal = 0;
            foreach (var insumoProducto in insumosProducto)
            {
                if (insumosDict.ContainsKey(insumoProducto.InsumoId))
                {
                    var insumo = insumosDict[insumoProducto.InsumoId];
                    costoTotal += insumoProducto.CantidadUtilizada * insumo.PrecioUnitario;
                }
            }

            // Obtener margen de ganancia de la empresa
            var empresa = await _empresaService.ObtenerEmpresaActualAsync();
            decimal margenGanancia = empresa?.MargenGanancia ?? 0;

            // Precio sugerido = Costo total * (1 + margen/100)
            decimal precioSugerido = costoTotal * (1 + margenGanancia / 100);

            return precioSugerido;
        }

        /// <summary>
        /// Valida si hay stock suficiente de todos los insumos necesarios para un producto
        /// </summary>
        public async Task<(bool StockSuficiente, List<string> InsumosInsuficientes)> ValidarStockDisponibleAsync(string productoId, decimal cantidadVenta = 1)
        {
            var insumosProducto = await ObtenerInsumosDelProductoAsync(productoId);
            if (!insumosProducto.Any())
            {
                return (true, new List<string>());
            }

            var todosInsumos = await _inventarioService.ObtenerInsumosAsync();
            var insumosDict = todosInsumos.ToDictionary(i => i.Id!, i => i);

            var insumosInsuficientes = new List<string>();

            foreach (var insumoProducto in insumosProducto)
            {
                if (insumosDict.ContainsKey(insumoProducto.InsumoId))
                {
                    var insumo = insumosDict[insumoProducto.InsumoId];
                    var cantidadNecesaria = insumoProducto.CantidadUtilizada * cantidadVenta;

                    if (insumo.Cantidad < cantidadNecesaria)
                    {
                        insumosInsuficientes.Add($"{insumo.Nombre} (Necesario: {cantidadNecesaria:N2} {insumo.UnidadMedida}, Disponible: {insumo.Cantidad:N2} {insumo.UnidadMedida})");
                    }
                }
            }

            return (insumosInsuficientes.Count == 0, insumosInsuficientes);
        }

        /// <summary>
        /// Descuenta el inventario cuando se realiza una venta (RF-21)
        /// </summary>
        public async Task<bool> DescontarInventarioPorVentaAsync(string productoId, decimal cantidadVenta, string motivo = "Venta")
        {
            var insumosProducto = await ObtenerInsumosDelProductoAsync(productoId);
            if (!insumosProducto.Any())
            {
                return true; // No hay insumos que descontar
            }

            var todosInsumos = await _inventarioService.ObtenerInsumosAsync();
            var insumosDict = todosInsumos.ToDictionary(i => i.Id!, i => i);

            foreach (var insumoProducto in insumosProducto)
            {
                if (insumosDict.ContainsKey(insumoProducto.InsumoId))
                {
                    var insumo = insumosDict[insumoProducto.InsumoId];
                    var cantidadADescontar = insumoProducto.CantidadUtilizada * cantidadVenta;
                    var nuevaCantidad = insumo.Cantidad - cantidadADescontar;

                    if (nuevaCantidad < 0)
                    {
                        throw new InvalidOperationException($"Stock insuficiente para {insumo.Nombre}. Disponible: {insumo.Cantidad:N2}, Necesario: {cantidadADescontar:N2}");
                    }

                    await _inventarioService.ActualizarStockAsync(
                        insumo.Id!,
                        nuevaCantidad,
                        "salida",
                        $"{motivo} - Producto: {productoId}, Cantidad: {cantidadVenta}"
                    );
                }
            }

            return true;
        }
    }
}

