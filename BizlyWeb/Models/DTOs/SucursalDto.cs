namespace BizlyWeb.Models.DTOs
{
    /// <summary>
    /// Representación de sucursal según API Bizly.
    /// </summary>
    public class SucursalDto
    {
        public string? Id { get; set; }
        public string EmpresaId { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string Ciudad { get; set; } = string.Empty;
        public decimal Latitud { get; set; }
        public decimal Longitud { get; set; }
        public string Departamento { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}


