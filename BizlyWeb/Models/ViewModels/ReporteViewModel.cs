using BizlyWeb.Models.DTOs;

namespace BizlyWeb.Models.ViewModels
{
    /// <summary>
    /// ViewModel para la vista Index de Reportes - Capa de Presentación
    /// </summary>
    public class ReporteIndexViewModel
    {
        public ReporteVentasDto ReporteVentas { get; set; } = new();
        public ReporteCostosGastosDto ReporteCostosGastos { get; set; } = new();
        public List<TopProductoDto> TopProductos { get; set; } = new();
        public List<TopClienteDto> TopClientes { get; set; } = new();
        public decimal MargenGananciaPromedio { get; set; }
        public PuntoEquilibrioDto PuntoEquilibrio { get; set; } = new();
        public ComparativaMensualDto ComparativaMensual { get; set; } = new();
        public DateTime? FiltroFechaInicio { get; set; }
        public DateTime? FiltroFechaFin { get; set; }
    }

    /// <summary>
    /// ViewModel para la vista de Métricas - Capa de Presentación
    /// </summary>
    public class MetricasViewModel
    {
        public decimal MargenGananciaPromedio { get; set; }
        public PuntoEquilibrioDto PuntoEquilibrio { get; set; } = new();
        public MetaMensualDto? MetaMensual { get; set; }
        public DateTime? FiltroFechaInicio { get; set; }
        public DateTime? FiltroFechaFin { get; set; }
        public decimal? MetaMensualInput { get; set; }
    }
}

