namespace BizlyWeb.Models.ViewModels
{
    /// <summary>
    /// ViewModel para la vista Index de Costos y Gastos - Capa de Presentación
    /// </summary>
    public class CostoGastoIndexViewModel
    {
        public List<CostoGastoViewModel> CostosGastos { get; set; } = new();
        public List<AlertaIncrementoCostoViewModel> AlertasIncremento { get; set; } = new();

        // Filtros
        public DateTime? FiltroFechaInicio { get; set; }
        public DateTime? FiltroFechaFin { get; set; }
        public string? FiltroCategoriaFinanciera { get; set; }
        public string? FiltroClasificacion { get; set; }
        public string? FiltroSucursalId { get; set; }

        // Resumen
        public decimal TotalCostosGastos { get; set; }
        public decimal TotalDirectos { get; set; }
        public decimal TotalAdministrativos { get; set; }
        public decimal TotalFijos { get; set; }
        public decimal TotalVariables { get; set; }
    }

    /// <summary>
    /// ViewModel para alertas de incremento de costos - Capa de Presentación
    /// </summary>
    public class AlertaIncrementoCostoViewModel
    {
        public string Categoria { get; set; } = string.Empty;
        public decimal MontoAnterior { get; set; }
        public decimal MontoActual { get; set; }
        public decimal PorcentajeIncremento { get; set; }
    }
}


