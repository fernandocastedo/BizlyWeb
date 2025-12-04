using BizlyWeb.Models.DTOs;
using Microsoft.AspNetCore.Http;

namespace BizlyWeb.Services
{
    /// <summary>
    /// Servicio de negocio para generación de reportes y métricas financieras.
    /// Capa de Negocio
    /// </summary>
    public class ReporteService
    {
        private readonly VentaService _ventaService;
        private readonly CostoGastoService _costoGastoService;
        private readonly ClienteService _clienteService;
        private readonly ProductoService _productoService;
        private readonly EmpresaService _empresaService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ReporteService(
            VentaService ventaService,
            CostoGastoService costoGastoService,
            ClienteService clienteService,
            ProductoService productoService,
            EmpresaService empresaService,
            IHttpContextAccessor httpContextAccessor)
        {
            _ventaService = ventaService;
            _costoGastoService = costoGastoService;
            _clienteService = clienteService;
            _productoService = productoService;
            _empresaService = empresaService;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Genera reporte de ventas (RF-38)
        /// </summary>
        public async Task<ReporteVentasDto> GenerarReporteVentasAsync(DateTime? fechaInicio, DateTime? fechaFin)
        {
            var ventas = await _ventaService.ObtenerVentasFiltradasAsync(fechaInicio, fechaFin, estadoPedido: "completado");
            
            return new ReporteVentasDto
            {
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                TotalVentas = ventas.Count,
                TotalIngresos = ventas.Sum(v => v.Total),
                PromedioVenta = ventas.Any() ? ventas.Average(v => v.Total) : 0,
                VentasPorDia = ventas.GroupBy(v => v.Fecha.Date)
                    .Select(g => new VentasPorDiaDto
                    {
                        Fecha = g.Key,
                        Cantidad = g.Count(),
                        Total = g.Sum(v => v.Total)
                    })
                    .OrderBy(v => v.Fecha)
                    .ToList()
            };
        }

        /// <summary>
        /// Genera reporte de costos y gastos (RF-39)
        /// </summary>
        public async Task<ReporteCostosGastosDto> GenerarReporteCostosGastosAsync(DateTime? fechaInicio, DateTime? fechaFin)
        {
            var costosGastos = await _costoGastoService.ObtenerCostosGastosFiltradosAsync(fechaInicio, fechaFin);
            
            return new ReporteCostosGastosDto
            {
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                TotalCostosGastos = costosGastos.Sum(cg => cg.Monto),
                TotalDirectos = costosGastos.Where(cg => cg.CategoriaFinanciera == "DIRECTO").Sum(cg => cg.Monto),
                TotalAdministrativos = costosGastos.Where(cg => cg.CategoriaFinanciera == "ADMINISTRATIVO").Sum(cg => cg.Monto),
                TotalFijos = costosGastos.Where(cg => cg.Clasificacion == "FIJO").Sum(cg => cg.Monto),
                TotalVariables = costosGastos.Where(cg => cg.Clasificacion == "VARIABLE").Sum(cg => cg.Monto),
                CostosPorDia = costosGastos.GroupBy(cg => cg.Fecha.Date)
                    .Select(g => new CostosPorDiaDto
                    {
                        Fecha = g.Key,
                        Total = g.Sum(cg => cg.Monto)
                    })
                    .OrderBy(c => c.Fecha)
                    .ToList()
            };
        }

        /// <summary>
        /// Obtiene top productos más vendidos (RF-40)
        /// </summary>
        public async Task<List<TopProductoDto>> ObtenerTopProductosAsync(DateTime? fechaInicio, DateTime? fechaFin, int top = 10)
        {
            var ventas = await _ventaService.ObtenerVentasFiltradasAsync(fechaInicio, fechaFin, estadoPedido: "completado");
            var detallesVenta = new List<DetalleVentaDto>();
            
            foreach (var venta in ventas)
            {
                var detalles = await _ventaService.ObtenerDetallesVentaAsync(venta.Id!);
                detallesVenta.AddRange(detalles);
            }

            var topProductos = detallesVenta
                .GroupBy(d => d.ProductoVentaId)
                .Select(g => new
                {
                    ProductoId = g.Key,
                    CantidadVendida = g.Sum(d => d.Cantidad),
                    TotalIngresos = g.Sum(d => d.Subtotal)
                })
                .OrderByDescending(x => x.TotalIngresos)
                .Take(top)
                .ToList();

            var productos = await _productoService.ObtenerProductosAsync();
            var productosDict = productos.ToDictionary(p => p.Id!, p => p);

            return topProductos.Select(tp => new TopProductoDto
            {
                ProductoId = tp.ProductoId,
                ProductoNombre = productosDict.ContainsKey(tp.ProductoId) ? productosDict[tp.ProductoId].Nombre : "N/A",
                CantidadVendida = tp.CantidadVendida,
                TotalIngresos = tp.TotalIngresos
            }).ToList();
        }

        /// <summary>
        /// Obtiene top clientes (RF-40) - ya implementado en ClienteService, pero lo exponemos aquí también
        /// </summary>
        public async Task<List<TopClienteDto>> ObtenerTopClientesAsync(DateTime? fechaInicio, DateTime? fechaFin, int top = 10)
        {
            var topClientes = await _clienteService.ObtenerTopClientesAsync(fechaInicio, fechaFin);
            return topClientes.Take(top).Select(tc => new TopClienteDto
            {
                ClienteId = tc.ClienteId,
                ClienteNombre = tc.ClienteNombre,
                TotalCompras = tc.TotalCompras,
                TotalGastado = tc.TotalGastado
            }).ToList();
        }

        /// <summary>
        /// Calcula el margen de ganancia promedio (RF-41)
        /// </summary>
        public async Task<decimal> CalcularMargenGananciaPromedioAsync(DateTime? fechaInicio, DateTime? fechaFin)
        {
            var empresa = await _empresaService.ObtenerEmpresaActualAsync();
            if (empresa == null)
            {
                return 0;
            }

            // El margen de ganancia promedio es el configurado en la empresa
            // Si se requiere calcular basado en ventas reales, se necesitaría:
            // (Ingresos - Costos) / Ingresos * 100
            var ventas = await _ventaService.ObtenerVentasFiltradasAsync(fechaInicio, fechaFin, estadoPedido: "completado");
            var costosGastos = await _costoGastoService.ObtenerCostosGastosFiltradosAsync(fechaInicio, fechaFin);
            
            var ingresos = ventas.Sum(v => v.Total);
            var costos = costosGastos.Sum(cg => cg.Monto);
            
            if (ingresos == 0)
            {
                return empresa.MargenGanancia; // Retornar el configurado si no hay ventas
            }

            var margenReal = ((ingresos - costos) / ingresos) * 100;
            return margenReal;
        }

        /// <summary>
        /// Calcula el punto de equilibrio (RF-42)
        /// Punto de equilibrio = Costos Fijos / (1 - (Costos Variables / Ingresos))
        /// </summary>
        public async Task<PuntoEquilibrioDto> CalcularPuntoEquilibrioAsync(DateTime? fechaInicio, DateTime? fechaFin)
        {
            var costosGastos = await _costoGastoService.ObtenerCostosGastosFiltradosAsync(fechaInicio, fechaFin);
            var ventas = await _ventaService.ObtenerVentasFiltradasAsync(fechaInicio, fechaFin, estadoPedido: "completado");
            
            var costosFijos = costosGastos.Where(cg => cg.Clasificacion == "FIJO").Sum(cg => cg.Monto);
            var costosVariables = costosGastos.Where(cg => cg.Clasificacion == "VARIABLE").Sum(cg => cg.Monto);
            var ingresos = ventas.Sum(v => v.Total);

            decimal puntoEquilibrio = 0;
            if (ingresos > 0 && costosVariables < ingresos)
            {
                var margenContribucion = 1 - (costosVariables / ingresos);
                if (margenContribucion > 0)
                {
                    puntoEquilibrio = costosFijos / margenContribucion;
                }
            }

            return new PuntoEquilibrioDto
            {
                CostosFijos = costosFijos,
                CostosVariables = costosVariables,
                Ingresos = ingresos,
                PuntoEquilibrio = puntoEquilibrio,
                MargenContribucion = ingresos > 0 ? (1 - (costosVariables / ingresos)) * 100 : 0
            };
        }

        /// <summary>
        /// Calcula el progreso hacia la meta mensual de ventas (RF-43)
        /// </summary>
        public async Task<MetaMensualDto> CalcularMetaMensualAsync(decimal metaMensual, int mes, int año)
        {
            var inicioMes = new DateTime(año, mes, 1);
            var finMes = inicioMes.AddMonths(1).AddSeconds(-1);
            
            var ventas = await _ventaService.ObtenerVentasFiltradasAsync(inicioMes, finMes, estadoPedido: "completado");
            var ingresosActuales = ventas.Sum(v => v.Total);
            
            var diasTranscurridos = DateTime.Now.Day;
            var diasTotales = DateTime.DaysInMonth(año, mes);
            var porcentajeTiempo = (diasTranscurridos / (decimal)diasTotales) * 100;
            
            var porcentajeMeta = metaMensual > 0 ? (ingresosActuales / metaMensual) * 100 : 0;
            var proyeccion = diasTranscurridos > 0 ? (ingresosActuales / diasTranscurridos) * diasTotales : 0;
            
            return new MetaMensualDto
            {
                MetaMensual = metaMensual,
                IngresosActuales = ingresosActuales,
                PorcentajeMeta = porcentajeMeta,
                PorcentajeTiempo = porcentajeTiempo,
                Proyeccion = proyeccion,
                Diferencia = ingresosActuales - metaMensual,
                Mes = mes,
                Año = año
            };
        }

        /// <summary>
        /// Genera comparativa mensual de desempeño (RF-44)
        /// </summary>
        public async Task<ComparativaMensualDto> GenerarComparativaMensualAsync(int meses = 6)
        {
            var ahora = DateTime.UtcNow;
            var comparativa = new ComparativaMensualDto
            {
                Meses = new List<MesComparativaDto>()
            };

            for (int i = meses - 1; i >= 0; i--)
            {
                var fecha = ahora.AddMonths(-i);
                var inicioMes = new DateTime(fecha.Year, fecha.Month, 1);
                var finMes = inicioMes.AddMonths(1).AddSeconds(-1);

                var ventas = await _ventaService.ObtenerVentasFiltradasAsync(inicioMes, finMes, estadoPedido: "completado");
                var costosGastos = await _costoGastoService.ObtenerCostosGastosFiltradosAsync(inicioMes, finMes);

                var ingresos = ventas.Sum(v => v.Total);
                var costos = costosGastos.Sum(cg => cg.Monto);
                var ganancia = ingresos - costos;

                comparativa.Meses.Add(new MesComparativaDto
                {
                    Mes = fecha.Month,
                    Año = fecha.Year,
                    NombreMes = fecha.ToString("MMMM yyyy"),
                    Ingresos = ingresos,
                    Costos = costos,
                    Ganancia = ganancia,
                    TotalVentas = ventas.Count
                });
            }

            return comparativa;
        }
    }
}


