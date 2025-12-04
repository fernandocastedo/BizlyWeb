namespace BizlyWeb.Models.DTOs
{
    /// <summary>
    /// Representación de la empresa consumida desde la API Bizly.
    /// </summary>
    public class EmpresaDto
    {
        public string? Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Rubro { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal MargenGanancia { get; set; }
        public string LogoUrl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}


