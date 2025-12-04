namespace BizlyWeb.Models.ViewModels
{
    /// <summary>
    /// ViewModel para Venta - Capa de Datos
    /// </summary>
    public class VentaViewModel
    {
        public string? Id { get; set; }
        public string? ClienteId { get; set; }
        public string? ClienteNombre { get; set; }
        public DateTime Fecha { get; set; }
        public string MetodoPago { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public bool EsEnvio { get; set; }
        public string EstadoPago { get; set; } = string.Empty;
        public string EstadoPedido { get; set; } = string.Empty;
        public string? UsuarioNombre { get; set; }
        public List<DetalleVentaViewModel> Detalles { get; set; } = new();
    }

    /// <summary>
    /// ViewModel para DetalleVenta - Capa de Datos
    /// </summary>
    public class DetalleVentaViewModel
    {
        public string? Id { get; set; }
        public string ProductoVentaId { get; set; } = string.Empty;
        public string ProductoNombre { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }
}


