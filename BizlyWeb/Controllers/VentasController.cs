using BizlyWeb.Attributes;
using BizlyWeb.Models.DTOs;
using BizlyWeb.Models.ViewModels;
using BizlyWeb.Services;
using Microsoft.AspNetCore.Mvc;

namespace BizlyWeb.Controllers
{
    [AuthorizeRole("EMPRENDEDOR", "TRABAJADOR")]
    public class VentasController : Controller
    {
        private readonly VentaService _ventaService;
        private readonly ProductoService _productoService;
        private readonly ILogger<VentasController> _logger;

        public VentasController(
            VentaService ventaService,
            ProductoService productoService,
            ILogger<VentasController> logger)
        {
            _ventaService = ventaService;
            _productoService = productoService;
            _logger = logger;
        }

        /// <summary>
        /// Vista principal: Historial de ventas con filtros (RF-26)
        /// </summary>
        public async Task<IActionResult> Index(DateTime? fechaInicio, DateTime? fechaFin, string? estadoPedido, string? estadoPago)
        {
            try
            {
                var ventas = await _ventaService.ObtenerVentasFiltradasAsync(fechaInicio, fechaFin, estadoPedido, estadoPago);
                var topVendedores = await _ventaService.ObtenerTopVendedoresAsync(fechaInicio, fechaFin);

                // Convertir a ViewModels
                var ventasViewModel = new List<VentaViewModel>();
                foreach (var venta in ventas)
                {
                    var detalles = await _ventaService.ObtenerDetallesVentaAsync(venta.Id!);
                    var productos = await _productoService.ObtenerProductosAsync();
                    var productosDict = productos.ToDictionary(p => p.Id!, p => p);

                    ventasViewModel.Add(new VentaViewModel
                    {
                        Id = venta.Id,
                        ClienteId = venta.ClienteId,
                        Fecha = venta.Fecha,
                        MetodoPago = venta.MetodoPago,
                        Total = venta.Total,
                        EsEnvio = venta.EsEnvio,
                        EstadoPago = venta.EstadoPago,
                        EstadoPedido = venta.EstadoPedido,
                        Detalles = detalles.Select(d =>
                        {
                            var producto = productosDict.ContainsKey(d.ProductoVentaId) ? productosDict[d.ProductoVentaId] : null;
                            return new DetalleVentaViewModel
                            {
                                Id = d.Id,
                                ProductoVentaId = d.ProductoVentaId,
                                ProductoNombre = producto?.Nombre ?? "N/A",
                                Cantidad = d.Cantidad,
                                PrecioUnitario = d.PrecioUnitario,
                                Subtotal = d.Subtotal
                            };
                        }).ToList()
                    });
                }

                var viewModel = new VentaIndexViewModel
                {
                    Ventas = ventasViewModel,
                    FiltroFechaInicio = fechaInicio,
                    FiltroFechaFin = fechaFin,
                    FiltroEstadoPedido = estadoPedido,
                    FiltroEstadoPago = estadoPago,
                    TotalVentas = ventasViewModel.Count,
                    TotalIngresos = ventasViewModel.Sum(v => v.Total),
                    TopVendedores = topVendedores.Select(tv => new TopVendedorViewModel
                    {
                        UsuarioId = tv.UsuarioId,
                        UsuarioNombre = tv.UsuarioNombre,
                        TotalVentas = tv.TotalVentas,
                        TotalIngresos = tv.TotalIngresos
                    }).ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ventas");
                TempData["ErrorMessage"] = "Ocurrió un error al cargar las ventas.";
                return View(new VentaIndexViewModel());
            }
        }

        /// <summary>
        /// Punto de Venta (POS) - RF-22
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            try
            {
                var productos = await _productoService.ObtenerProductosAsync();
                var productosActivos = productos.Where(p => p.Activo).ToList();

                // TODO: Obtener clientes cuando se implemente el módulo de Clientes
                var clientes = new List<ClienteViewModel>();

                var viewModel = new PuntoVentaViewModel
                {
                    Productos = productosActivos.Select(p => new ProductoVentaViewModel
                    {
                        Id = p.Id,
                        Nombre = p.Nombre,
                        Descripcion = p.Descripcion,
                        PrecioVenta = p.PrecioVenta
                    }).ToList(),
                    Clientes = clientes,
                    Carrito = new List<ItemCarritoViewModel>(),
                    Total = 0
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar punto de venta");
                TempData["ErrorMessage"] = "Ocurrió un error al cargar el punto de venta.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Procesa la creación de una venta desde el POS
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            string? clienteId,
            string metodoPago,
            bool esEnvio,
            List<string>? productoIds,
            List<int>? cantidades)
        {
            if (productoIds == null || cantidades == null || productoIds.Count != cantidades.Count)
            {
                TempData["ErrorMessage"] = "Debe agregar al menos un producto al carrito.";
                return RedirectToAction(nameof(Create));
            }

            try
            {
                var productos = await _productoService.ObtenerProductosAsync();
                var productosDict = productos.ToDictionary(p => p.Id!, p => p);

                var detalles = new List<DetalleVentaDto>();
                for (int i = 0; i < productoIds.Count; i++)
                {
                    if (!string.IsNullOrEmpty(productoIds[i]) && cantidades[i] > 0)
                    {
                        if (productosDict.ContainsKey(productoIds[i]))
                        {
                            var producto = productosDict[productoIds[i]];
                            detalles.Add(new DetalleVentaDto
                            {
                                ProductoVentaId = productoIds[i],
                                Cantidad = cantidades[i],
                                PrecioUnitario = producto.PrecioVenta,
                                Subtotal = producto.PrecioVenta * cantidades[i]
                            });
                        }
                    }
                }

                if (!detalles.Any())
                {
                    TempData["ErrorMessage"] = "Debe agregar al menos un producto válido al carrito.";
                    return RedirectToAction(nameof(Create));
                }

                var venta = await _ventaService.CrearVentaAsync(clienteId, metodoPago, esEnvio, detalles);
                if (venta != null)
                {
                    TempData["SuccessMessage"] = $"Venta creada correctamente. Total: {venta.Total:C}";
                    return RedirectToAction(nameof(Detalle), new { id = venta.Id });
                }
                else
                {
                    TempData["ErrorMessage"] = "No se pudo crear la venta.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear venta");
                TempData["ErrorMessage"] = $"Error al crear la venta: {ex.Message}";
            }

            return RedirectToAction(nameof(Create));
        }

        /// <summary>
        /// Vista de detalle de una venta
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Detalle(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "Id de venta inválido.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var venta = await _ventaService.ObtenerVentaPorIdAsync(id);
                if (venta == null)
                {
                    TempData["ErrorMessage"] = "Venta no encontrada.";
                    return RedirectToAction(nameof(Index));
                }

                var detalles = await _ventaService.ObtenerDetallesVentaAsync(id);
                var productos = await _productoService.ObtenerProductosAsync();
                var productosDict = productos.ToDictionary(p => p.Id!, p => p);

                var viewModel = new VentaViewModel
                {
                    Id = venta.Id,
                    ClienteId = venta.ClienteId,
                    Fecha = venta.Fecha,
                    MetodoPago = venta.MetodoPago,
                    Total = venta.Total,
                    EsEnvio = venta.EsEnvio,
                    EstadoPago = venta.EstadoPago,
                    EstadoPedido = venta.EstadoPedido,
                    Detalles = detalles.Select(d =>
                    {
                        var producto = productosDict.ContainsKey(d.ProductoVentaId) ? productosDict[d.ProductoVentaId] : null;
                        return new DetalleVentaViewModel
                        {
                            Id = d.Id,
                            ProductoVentaId = d.ProductoVentaId,
                            ProductoNombre = producto?.Nombre ?? "N/A",
                            Cantidad = d.Cantidad,
                            PrecioUnitario = d.PrecioUnitario,
                            Subtotal = d.Subtotal
                        };
                    }).ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar detalle de venta {VentaId}", id);
                TempData["ErrorMessage"] = "Ocurrió un error al cargar la venta.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Vista de pedidos pendientes (RF-30)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> PedidosPendientes()
        {
            try
            {
                var pedidos = await _ventaService.ObtenerPedidosPendientesAsync();
                var productos = await _productoService.ObtenerProductosAsync();
                var productosDict = productos.ToDictionary(p => p.Id!, p => p);

                var pedidosViewModel = new List<VentaViewModel>();
                foreach (var pedido in pedidos)
                {
                    var detalles = await _ventaService.ObtenerDetallesVentaAsync(pedido.Id!);
                    pedidosViewModel.Add(new VentaViewModel
                    {
                        Id = pedido.Id,
                        ClienteId = pedido.ClienteId,
                        Fecha = pedido.Fecha,
                        MetodoPago = pedido.MetodoPago,
                        Total = pedido.Total,
                        EsEnvio = pedido.EsEnvio,
                        EstadoPago = pedido.EstadoPago,
                        EstadoPedido = pedido.EstadoPedido,
                        Detalles = detalles.Select(d =>
                        {
                            var producto = productosDict.ContainsKey(d.ProductoVentaId) ? productosDict[d.ProductoVentaId] : null;
                            return new DetalleVentaViewModel
                            {
                                Id = d.Id,
                                ProductoVentaId = d.ProductoVentaId,
                                ProductoNombre = producto?.Nombre ?? "N/A",
                                Cantidad = d.Cantidad,
                                PrecioUnitario = d.PrecioUnitario,
                                Subtotal = d.Subtotal
                            };
                        }).ToList()
                    });
                }

                return View(pedidosViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener pedidos pendientes");
                TempData["ErrorMessage"] = "Ocurrió un error al cargar los pedidos pendientes.";
                return View(new List<VentaViewModel>());
            }
        }

        /// <summary>
        /// Actualiza el estado de un pedido (RF-31)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarEstado(string id, string estadoPedido, string? estadoPago)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "Id de venta inválido.";
                return RedirectToAction(nameof(PedidosPendientes));
            }

            try
            {
                var resultado = await _ventaService.ActualizarEstadoVentaAsync(id, estadoPedido, estadoPago);
                TempData[resultado ? "SuccessMessage" : "ErrorMessage"] =
                    resultado ? "Estado actualizado correctamente." : "No se pudo actualizar el estado.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar estado de venta {VentaId}", id);
                TempData["ErrorMessage"] = $"Error al actualizar el estado: {ex.Message}";
            }

            return RedirectToAction(nameof(PedidosPendientes));
        }

        /// <summary>
        /// Cancela una venta (RF-28)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancelar(string id, string motivo)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "Id de venta inválido.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var resultado = await _ventaService.CancelarVentaAsync(id, motivo);
                TempData[resultado ? "SuccessMessage" : "ErrorMessage"] =
                    resultado ? "Venta cancelada correctamente. El stock ha sido devuelto al inventario." : "No se pudo cancelar la venta.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cancelar venta {VentaId}", id);
                TempData["ErrorMessage"] = $"Error al cancelar la venta: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}


