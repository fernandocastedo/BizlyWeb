namespace BizlyWeb.Models.DTOs
{
    /// <summary>
    /// DTO para RegistroInventario - Capa de Datos
    /// </summary>
    public class RegistroInventarioDto
    {
        public string? Id { get; set; }
        public string EmpresaId { get; set; } = string.Empty;
        public string SucursalId { get; set; } = string.Empty;
        public string UsuarioId { get; set; } = string.Empty;
        public string InsumoId { get; set; } = string.Empty;
        public string TipoMovimiento { get; set; } = string.Empty; // "entrada" / "salida" / "ajuste"
        public decimal CantidadAnterior { get; set; }
        public decimal CantidadNueva { get; set; }
        public string Motivo { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}

