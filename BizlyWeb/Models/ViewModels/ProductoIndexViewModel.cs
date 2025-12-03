namespace BizlyWeb.Models.ViewModels
{
    /// <summary>
    /// ViewModel para la vista Index de Productos - Capa de Datos
    /// </summary>
    public class ProductoIndexViewModel
    {
        public List<ProductoVentaViewModel> Productos { get; set; } = new();
        public List<CategoriaViewModel> Categorias { get; set; } = new();
        public string? FiltroNombre { get; set; }
        public string? FiltroCategoriaId { get; set; }
        public bool? FiltroSoloActivos { get; set; }
        public int TotalProductos { get; set; }
    }
}

