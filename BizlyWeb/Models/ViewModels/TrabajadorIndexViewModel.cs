namespace BizlyWeb.Models.ViewModels
{
    /// <summary>
    /// ViewModel para la vista Index de Trabajadores - Capa de Presentación
    /// </summary>
    public class TrabajadorIndexViewModel
    {
        public List<TrabajadorViewModel> Trabajadores { get; set; } = new();
    }

    /// <summary>
    /// ViewModel para el reporte de desempeño de un trabajador - Capa de Presentación
    /// </summary>
    public class DesempenoTrabajadorViewModel
    {
        public string TrabajadorId { get; set; } = string.Empty;
        public string TrabajadorNombre { get; set; } = string.Empty;
        public string Cargo { get; set; } = string.Empty;
        public int TotalVentas { get; set; }
        public decimal TotalIngresos { get; set; }
        public decimal PromedioVenta { get; set; }
        public int VentasMesActual { get; set; }
        public decimal IngresosMesActual { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
    }
}


