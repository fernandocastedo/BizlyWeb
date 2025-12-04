namespace BizlyWeb.Models.DTOs
{
    /// <summary>
    /// DTO para CostoGasto - Capa de Datos
    /// </summary>
    public class CostoGastoDto
    {
        public string? Id { get; set; }
        public string EmpresaId { get; set; } = string.Empty;
        public string? SucursalId { get; set; }
        public string UsuarioId { get; set; } = string.Empty;
        public string CategoriaFinanciera { get; set; } = string.Empty; // "DIRECTO" / "ADMINISTRATIVO"
        public string Descripcion { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public DateTime Fecha { get; set; }
        public string Clasificacion { get; set; } = string.Empty; // "FIJO" / "VARIABLE"
        public string? InsumoId { get; set; }
        public string? TrabajadorId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}


