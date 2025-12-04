using BizlyWeb.Attributes;
using BizlyWeb.Models.DTOs;
using BizlyWeb.Models.ViewModels;
using BizlyWeb.Services;
using Microsoft.AspNetCore.Mvc;

namespace BizlyWeb.Controllers
{
    /// <summary>
    /// Controlador del Dashboard - Capa de Presentación
    /// </summary>
    [AuthorizeRole("EMPRENDEDOR", "TRABAJADOR")]
    public class DashboardController : Controller
    {
        private readonly VentaService _ventaService;
        private readonly InventarioService _inventarioService;
        private readonly CostoGastoService _costoGastoService;
        private readonly ProductoService _productoService;
        private readonly ClienteService _clienteService;
        private readonly TrabajadorService _trabajadorService;
        private readonly ReporteService _reporteService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            VentaService ventaService,
            InventarioService inventarioService,
            CostoGastoService costoGastoService,
            ProductoService productoService,
            ClienteService clienteService,
            TrabajadorService trabajadorService,
            ReporteService reporteService,
            ILogger<DashboardController> logger)
        {
            _ventaService = ventaService;
            _inventarioService = inventarioService;
            _costoGastoService = costoGastoService;
            _productoService = productoService;
            _clienteService = clienteService;
            _trabajadorService = trabajadorService;
            _reporteService = reporteService;
            _logger = logger;
        }

        /// <summary>
        /// Vista principal del Dashboard (RF-07)
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                var ahora = DateTime.Now;
                var inicioDia = new DateTime(ahora.Year, ahora.Month, ahora.Day);
                var finDia = inicioDia.AddDays(1).AddSeconds(-1);
                var inicioMes = new DateTime(ahora.Year, ahora.Month, 1);
                var finMes = inicioMes.AddMonths(1).AddSeconds(-1);
                var inicioSemana = ahora.AddDays(-7);

                // Ventas del día
                var ventasDelDia = await _ventaService.ObtenerVentasFiltradasAsync(inicioDia, finDia, estadoPedido: "completado");
                var ventasDelDiaTotal = ventasDelDia.Sum(v => v.Total);

                // Ventas del mes
                var ventasDelMes = await _ventaService.ObtenerVentasFiltradasAsync(inicioMes, finMes, estadoPedido: "completado");
                var ventasDelMesTotal = ventasDelMes.Sum(v => v.Total);

                // Costos del mes
                var costosDelMes = await _costoGastoService.ObtenerCostosGastosFiltradosAsync(inicioMes, finMes);
                var costosDelMesTotal = costosDelMes.Sum(cg => cg.Monto);

                // Productos activos
                var productos = await _productoService.ObtenerProductosAsync();
                var productosActivos = productos.Count(p => p.Activo);

                // Insumos con stock bajo
                var insumos = await _inventarioService.ObtenerInsumosAsync();
                var insumosStockBajo = insumos.Count(i => i.Cantidad <= i.StockMinimo);

                // Clientes y trabajadores
                var clientes = await _clienteService.ObtenerClientesAsync();
                var trabajadores = await _trabajadorService.ObtenerTrabajadoresAsync();

                // Top productos y clientes
                var topProductos = await _reporteService.ObtenerTopProductosAsync(inicioMes, finMes, 5);
                var topClientes = await _reporteService.ObtenerTopClientesAsync(inicioMes, finMes, 5);

                // Ventas recientes (últimas 5)
                var todasVentas = await _ventaService.ObtenerVentasAsync();
                var clientesDict = clientes.ToDictionary(c => c.Id ?? string.Empty, c => c.Nombre);
                
                var ventasRecientes = todasVentas
                    .OrderByDescending(v => v.Fecha)
                    .Take(5)
                    .Select(v => new VentaResumenDto
                    {
                        Id = v.Id ?? string.Empty,
                        Fecha = v.Fecha,
                        Total = v.Total,
                        ClienteNombre = !string.IsNullOrEmpty(v.ClienteId) && clientesDict.ContainsKey(v.ClienteId) 
                            ? clientesDict[v.ClienteId] 
                            : "Sin cliente",
                        EstadoPedido = v.EstadoPedido ?? "completado"
                    })
                    .ToList();

                // Ventas últimos 7 días para gráfica
                var ventasUltimos7Dias = await _ventaService.ObtenerVentasFiltradasAsync(inicioSemana, finDia, estadoPedido: "completado");
                var ventasPorDia = ventasUltimos7Dias
                    .GroupBy(v => v.Fecha.Date)
                    .Select(g => new VentasPorDiaDto
                    {
                        Fecha = g.Key,
                        Cantidad = g.Count(),
                        Total = g.Sum(v => v.Total)
                    })
                    .OrderBy(v => v.Fecha)
                    .ToList();

                // Alertas
                var alertas = new List<AlertaDashboardDto>();

                if (insumosStockBajo > 0)
                {
                    alertas.Add(new AlertaDashboardDto
                    {
                        Tipo = "warning",
                        Titulo = "Stock Bajo",
                        Mensaje = $"Tienes {insumosStockBajo} insumo(s) con stock bajo. Revisa tu inventario.",
                        Icono = "fa-exclamation-triangle"
                    });
                }

                if (ventasDelDiaTotal == 0 && ahora.Hour >= 12)
                {
                    alertas.Add(new AlertaDashboardDto
                    {
                        Tipo = "info",
                        Titulo = "Sin Ventas Hoy",
                        Mensaje = "Aún no has registrado ventas hoy.",
                        Icono = "fa-info-circle"
                    });
                }

                var gananciaDelMes = ventasDelMesTotal - costosDelMesTotal;
                if (gananciaDelMes < 0)
                {
                    alertas.Add(new AlertaDashboardDto
                    {
                        Tipo = "danger",
                        Titulo = "Pérdida del Mes",
                        Mensaje = $"Este mes tienes una pérdida de {Math.Abs(gananciaDelMes):C}. Revisa tus costos.",
                        Icono = "fa-exclamation-circle"
                    });
                }

                var viewModel = new DashboardViewModel
                {
                    VentasDelDia = ventasDelDiaTotal,
                    VentasDelDiaCantidad = ventasDelDia.Count,
                    VentasDelMes = ventasDelMesTotal,
                    VentasDelMesCantidad = ventasDelMes.Count,
                    CostosDelMes = costosDelMesTotal,
                    GananciaDelMes = gananciaDelMes,
                    ProductosActivos = productosActivos,
                    InsumosConStockBajo = insumosStockBajo,
                    TotalInsumos = insumos.Count,
                    TotalClientes = clientes.Count,
                    TotalTrabajadores = trabajadores.Count,
                    TopProductos = topProductos,
                    TopClientes = topClientes,
                    VentasRecientes = ventasRecientes,
                    Alertas = alertas,
                    VentasUltimos7Dias = ventasPorDia
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar dashboard");
                TempData["Error"] = "Error al cargar el dashboard. Por favor, intente nuevamente.";
                return View(new DashboardViewModel());
            }
        }
    }
}

