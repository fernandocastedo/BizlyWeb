namespace BizlyWeb.Models.DTOs
{
    /// <summary>
    /// DTO para Insumo - Capa de Datos
    /// </summary>
    public class InsumoDto
    {
        public string? Id { get; set; }
        public string EmpresaId { get; set; } = string.Empty;
        public string SucursalId { get; set; } = string.Empty;
        public string? CategoriaId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal Cantidad { get; set; }
        public string UnidadMedida { get; set; } = string.Empty;
        public decimal PrecioUnitario { get; set; }
        public decimal PrecioTotal { get; set; }
        public decimal StockMinimo { get; set; }
        public bool Activo { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

