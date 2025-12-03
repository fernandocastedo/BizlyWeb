namespace BizlyWeb.Models.ViewModels
{
    /// <summary>
    /// ViewModel para Insumo - Capa de Datos
    /// </summary>
    public class InsumoViewModel
    {
        public string? Id { get; set; }
        public string? CategoriaId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal Cantidad { get; set; }
        public string UnidadMedida { get; set; } = string.Empty;
        public decimal PrecioUnitario { get; set; }
        public decimal PrecioTotal { get; set; }
        public decimal StockMinimo { get; set; }
        public bool Activo { get; set; } = true;
        public bool StockBajo { get; set; }
        public string? CategoriaNombre { get; set; }
    }
}

