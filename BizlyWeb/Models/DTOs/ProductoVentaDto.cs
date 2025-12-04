namespace BizlyWeb.Models.DTOs
{
    /// <summary>
    /// DTO para ProductoVenta - Capa de Datos
    /// </summary>
    public class ProductoVentaDto
    {
        public string? Id { get; set; }
        public string EmpresaId { get; set; } = string.Empty;
        public string SucursalId { get; set; } = string.Empty;
        public string? CategoriaId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal PrecioVenta { get; set; }
        public bool Activo { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}


