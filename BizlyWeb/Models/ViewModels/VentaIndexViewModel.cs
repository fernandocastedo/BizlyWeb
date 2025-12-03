namespace BizlyWeb.Models.ViewModels
{
    /// <summary>
    /// ViewModel para la vista Index de Ventas - Capa de Datos
    /// </summary>
    public class VentaIndexViewModel
    {
        public List<VentaViewModel> Ventas { get; set; } = new();
        public DateTime? FiltroFechaInicio { get; set; }
        public DateTime? FiltroFechaFin { get; set; }
        public string? FiltroEstadoPedido { get; set; }
        public string? FiltroEstadoPago { get; set; }
        public int TotalVentas { get; set; }
        public decimal TotalIngresos { get; set; }
        public List<TopVendedorViewModel> TopVendedores { get; set; } = new();
    }

    /// <summary>
    /// ViewModel para Top de Vendedores
    /// </summary>
    public class TopVendedorViewModel
    {
        public string UsuarioId { get; set; } = string.Empty;
        public string UsuarioNombre { get; set; } = string.Empty;
        public int TotalVentas { get; set; }
        public decimal TotalIngresos { get; set; }
    }
}

