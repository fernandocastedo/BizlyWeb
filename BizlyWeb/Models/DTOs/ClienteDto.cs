namespace BizlyWeb.Models.DTOs
{
    /// <summary>
    /// DTO para Cliente - Capa de Datos
    /// </summary>
    public class ClienteDto
    {
        public string? Id { get; set; }
        public string EmpresaId { get; set; } = string.Empty;
        public string? SucursalId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int Nit { get; set; }
        public string Telefono { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}

