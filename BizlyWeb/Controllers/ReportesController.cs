using BizlyWeb.Attributes;
using BizlyWeb.Models.DTOs;
using BizlyWeb.Models.ViewModels;
using BizlyWeb.Services;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BizlyWeb.Controllers
{
    [AuthorizeRole("EMPRENDEDOR")]
    public class ReportesController : Controller
    {
        private readonly ReporteService _reporteService;
        private readonly ILogger<ReportesController> _logger;

        public ReportesController(
            ReporteService reporteService,
            ILogger<ReportesController> logger)
        {
            _reporteService = reporteService;
            _logger = logger;
        }

        /// <summary>
        /// Vista principal de reportes y métricas
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                var ahora = DateTime.Now;
                var inicioMes = new DateTime(ahora.Year, ahora.Month, 1);
                var finMes = inicioMes.AddMonths(1).AddSeconds(-1);

                var reporteVentas = await _reporteService.GenerarReporteVentasAsync(inicioMes, finMes);
                var reporteCostos = await _reporteService.GenerarReporteCostosGastosAsync(inicioMes, finMes);
                var topProductos = await _reporteService.ObtenerTopProductosAsync(inicioMes, finMes, 5);
                var topClientes = await _reporteService.ObtenerTopClientesAsync(inicioMes, finMes, 5);
                var margenGanancia = await _reporteService.CalcularMargenGananciaPromedioAsync(inicioMes, finMes);
                var puntoEquilibrio = await _reporteService.CalcularPuntoEquilibrioAsync(inicioMes, finMes);
                var comparativa = await _reporteService.GenerarComparativaMensualAsync(6);

                var viewModel = new ReporteIndexViewModel
                {
                    ReporteVentas = reporteVentas,
                    ReporteCostosGastos = reporteCostos,
                    TopProductos = topProductos,
                    TopClientes = topClientes,
                    MargenGananciaPromedio = margenGanancia,
                    PuntoEquilibrio = puntoEquilibrio,
                    ComparativaMensual = comparativa,
                    FiltroFechaInicio = inicioMes,
                    FiltroFechaFin = finMes
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar reportes");
                TempData["Error"] = "Error al cargar los reportes. Por favor, intente nuevamente.";
                return View(new ReporteIndexViewModel());
            }
        }

        /// <summary>
        /// Genera reporte de ventas con filtros (RF-38)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Ventas(DateTime? fechaInicio, DateTime? fechaFin)
        {
            try
            {
                var reporte = await _reporteService.GenerarReporteVentasAsync(fechaInicio, fechaFin);
                return View(reporte);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar reporte de ventas");
                TempData["Error"] = "Error al generar el reporte de ventas.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Genera reporte de costos y gastos con filtros (RF-39)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> CostosGastos(DateTime? fechaInicio, DateTime? fechaFin)
        {
            try
            {
                var reporte = await _reporteService.GenerarReporteCostosGastosAsync(fechaInicio, fechaFin);
                return View(reporte);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar reporte de costos y gastos");
                TempData["Error"] = "Error al generar el reporte de costos y gastos.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Vista de métricas financieras (RF-41, RF-42, RF-43)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Metricas(DateTime? fechaInicio, DateTime? fechaFin, decimal? metaMensual)
        {
            try
            {
                var ahora = fechaInicio?.Date ?? DateTime.Now;
                var margenGanancia = await _reporteService.CalcularMargenGananciaPromedioAsync(fechaInicio, fechaFin);
                var puntoEquilibrio = await _reporteService.CalcularPuntoEquilibrioAsync(fechaInicio, fechaFin);
                
                MetaMensualDto? metaMensualDto = null;
                if (metaMensual.HasValue && metaMensual.Value > 0)
                {
                    metaMensualDto = await _reporteService.CalcularMetaMensualAsync(metaMensual.Value, ahora.Month, ahora.Year);
                }

                var viewModel = new MetricasViewModel
                {
                    MargenGananciaPromedio = margenGanancia,
                    PuntoEquilibrio = puntoEquilibrio,
                    MetaMensual = metaMensualDto,
                    FiltroFechaInicio = fechaInicio,
                    FiltroFechaFin = fechaFin,
                    MetaMensualInput = metaMensual
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar métricas");
                TempData["Error"] = "Error al cargar las métricas.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Vista de comparativa mensual (RF-44)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Comparativa(int meses = 6)
        {
            try
            {
                var comparativa = await _reporteService.GenerarComparativaMensualAsync(meses);
                return View(comparativa);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar comparativa mensual");
                TempData["Error"] = "Error al generar la comparativa mensual.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Exporta reporte de ventas a PDF (RF-45)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ExportarVentasPDF(DateTime? fechaInicio, DateTime? fechaFin)
        {
            try
            {
                var reporte = await _reporteService.GenerarReporteVentasAsync(fechaInicio, fechaFin);
                
                QuestPDF.Settings.License = LicenseType.Community;
                
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(10));

                        page.Header()
                            .Text("Reporte de Ventas")
                            .SemiBold().FontSize(18).FontColor(Colors.Blue.Medium);

                        page.Content()
                            .PaddingVertical(1, Unit.Centimetre)
                            .Column(column =>
                            {
                                column.Item().Text($"Período: {reporte.FechaInicio?.ToString("dd/MM/yyyy") ?? "Inicio"} - {reporte.FechaFin?.ToString("dd/MM/yyyy") ?? "Fin"}");
                                column.Item().PaddingTop(10);
                                column.Item().Text($"Total de Ventas: {reporte.TotalVentas}");
                                column.Item().Text($"Total de Ingresos: {reporte.TotalIngresos:C}");
                                column.Item().Text($"Promedio por Venta: {reporte.PromedioVenta:C}");
                                
                                if (reporte.VentasPorDia.Any())
                                {
                                    column.Item().PaddingTop(10);
                                    column.Item().Text("Ventas por Día:").Bold();
                                    foreach (var venta in reporte.VentasPorDia)
                                    {
                                        column.Item().Text($"{venta.Fecha:dd/MM/yyyy}: {venta.Cantidad} ventas - {venta.Total:C}");
                                    }
                                }
                            });

                        page.Footer()
                            .AlignCenter()
                            .Text(x =>
                            {
                                x.Span("Generado el ");
                                x.Span($"{DateTime.Now:dd/MM/yyyy HH:mm}").SemiBold();
                            });
                    });
                });

                var stream = new MemoryStream();
                document.GeneratePdf(stream);
                stream.Position = 0;

                return File(stream.ToArray(), "application/pdf", $"Reporte_Ventas_{DateTime.Now:yyyyMMdd}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al exportar reporte de ventas a PDF");
                TempData["Error"] = "Error al exportar el reporte a PDF.";
                return RedirectToAction(nameof(Ventas), new { fechaInicio, fechaFin });
            }
        }

        /// <summary>
        /// Exporta reporte de costos y gastos a PDF (RF-45)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ExportarCostosPDF(DateTime? fechaInicio, DateTime? fechaFin)
        {
            try
            {
                var reporte = await _reporteService.GenerarReporteCostosGastosAsync(fechaInicio, fechaFin);
                
                QuestPDF.Settings.License = LicenseType.Community;
                
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(10));

                        page.Header()
                            .Text("Reporte de Costos y Gastos")
                            .SemiBold().FontSize(18).FontColor(Colors.Blue.Medium);

                        page.Content()
                            .PaddingVertical(1, Unit.Centimetre)
                            .Column(column =>
                            {
                                column.Item().Text($"Período: {reporte.FechaInicio?.ToString("dd/MM/yyyy") ?? "Inicio"} - {reporte.FechaFin?.ToString("dd/MM/yyyy") ?? "Fin"}");
                                column.Item().PaddingTop(10);
                                column.Item().Text($"Total Costos y Gastos: {reporte.TotalCostosGastos:C}");
                                column.Item().Text($"Total Directos: {reporte.TotalDirectos:C}");
                                column.Item().Text($"Total Administrativos: {reporte.TotalAdministrativos:C}");
                                column.Item().Text($"Total Fijos: {reporte.TotalFijos:C}");
                                column.Item().Text($"Total Variables: {reporte.TotalVariables:C}");
                            });

                        page.Footer()
                            .AlignCenter()
                            .Text(x =>
                            {
                                x.Span("Generado el ");
                                x.Span($"{DateTime.Now:dd/MM/yyyy HH:mm}").SemiBold();
                            });
                    });
                });

                var stream = new MemoryStream();
                document.GeneratePdf(stream);
                stream.Position = 0;

                return File(stream.ToArray(), "application/pdf", $"Reporte_Costos_{DateTime.Now:yyyyMMdd}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al exportar reporte de costos a PDF");
                TempData["Error"] = "Error al exportar el reporte a PDF.";
                return RedirectToAction(nameof(CostosGastos), new { fechaInicio, fechaFin });
            }
        }
    }
}


