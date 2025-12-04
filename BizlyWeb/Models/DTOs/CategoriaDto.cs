namespace BizlyWeb.Models.DTOs
{
    /// <summary>
    /// DTO para Categoria - Capa de Datos
    /// </summary>
    public class CategoriaDto
    {
        public string? Id { get; set; }
        public string EmpresaId { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}


