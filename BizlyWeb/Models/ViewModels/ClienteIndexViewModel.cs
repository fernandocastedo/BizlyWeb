namespace BizlyWeb.Models.ViewModels
{
    /// <summary>
    /// ViewModel para la vista Index de Clientes - Capa de Presentación
    /// </summary>
    public class ClienteIndexViewModel
    {
        public List<ClienteViewModel> Clientes { get; set; } = new();
        public List<TopClienteViewModel> TopClientes { get; set; } = new();

        // Filtros
        public string? FiltroNombre { get; set; }
        public int? FiltroNit { get; set; }
        public string? FiltroEmail { get; set; }
        public string? FiltroTelefono { get; set; }
        public DateTime? FiltroFechaInicio { get; set; }
        public DateTime? FiltroFechaFin { get; set; }
    }

    /// <summary>
    /// ViewModel para Top Cliente - Capa de Presentación
    /// </summary>
    public class TopClienteViewModel
    {
        public string ClienteId { get; set; } = string.Empty;
        public string ClienteNombre { get; set; } = string.Empty;
        public int TotalCompras { get; set; }
        public decimal TotalGastado { get; set; }
    }
}

