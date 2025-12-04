namespace BizlyWeb.Models.DTOs
{
    /// <summary>
    /// DTO para DetalleVenta - Capa de Datos
    /// </summary>
    public class DetalleVentaDto
    {
        public string? Id { get; set; }
        public string VentaId { get; set; } = string.Empty;
        public string ProductoVentaId { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }
}


