namespace BizlyWeb.Models.ViewModels
{
    /// <summary>
    /// ViewModel para ProductoVenta - Capa de Datos
    /// </summary>
    public class ProductoVentaViewModel
    {
        public string? Id { get; set; }
        public string? CategoriaId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal PrecioVenta { get; set; }
        public decimal PrecioSugerido { get; set; }
        public bool Activo { get; set; } = true;
        public string? CategoriaNombre { get; set; }
        public List<InsumoProductoViewModel> Insumos { get; set; } = new();
    }

    /// <summary>
    /// ViewModel para la relación Insumo-Producto
    /// </summary>
    public class InsumoProductoViewModel
    {
        public string? Id { get; set; }
        public string InsumoId { get; set; } = string.Empty;
        public string InsumoNombre { get; set; } = string.Empty;
        public decimal CantidadUtilizada { get; set; }
        public decimal PrecioUnitarioInsumo { get; set; }
        public decimal CostoTotal { get; set; }
        public decimal StockDisponible { get; set; }
        public bool StockSuficiente { get; set; }
    }
}


