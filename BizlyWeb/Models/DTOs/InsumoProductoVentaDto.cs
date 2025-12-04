namespace BizlyWeb.Models.DTOs
{
    /// <summary>
    /// DTO para InsumoProductoVenta - Capa de Datos
    /// </summary>
    public class InsumoProductoVentaDto
    {
        public string? Id { get; set; }
        public string ProductoVentaId { get; set; } = string.Empty;
        public string InsumoId { get; set; } = string.Empty;
        public decimal CantidadUtilizada { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}


