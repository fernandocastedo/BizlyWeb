namespace BizlyWeb.Models.DTOs
{
    /// <summary>
    /// DTO para Trabajador - Capa de Datos
    /// </summary>
    public class TrabajadorDto
    {
        public string? Id { get; set; }
        public string EmpresaId { get; set; } = string.Empty;
        public string? SucursalId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Cargo { get; set; } = string.Empty;
        public decimal SueldoMensual { get; set; }
        public string TipoGasto { get; set; } = string.Empty; // "fijo" / "variable"
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

