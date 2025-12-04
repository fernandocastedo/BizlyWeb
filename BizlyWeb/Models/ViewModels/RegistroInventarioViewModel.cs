namespace BizlyWeb.Models.ViewModels
{
    /// <summary>
    /// ViewModel para RegistroInventario - Capa de Datos
    /// </summary>
    public class RegistroInventarioViewModel
    {
        public string? Id { get; set; }
        public string InsumoId { get; set; } = string.Empty;
        public string InsumoNombre { get; set; } = string.Empty;
        public string TipoMovimiento { get; set; } = string.Empty;
        public decimal CantidadAnterior { get; set; }
        public decimal CantidadNueva { get; set; }
        public decimal Diferencia { get; set; }
        public string Motivo { get; set; } = string.Empty;
        public string UsuarioNombre { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}


