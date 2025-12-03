namespace BizlyWeb.Models.ViewModels
{
    /// <summary>
    /// ViewModel para la vista Index de Inventario - Capa de Datos
    /// </summary>
    public class InventarioIndexViewModel
    {
        public List<InsumoViewModel> Insumos { get; set; } = new();
        public List<CategoriaViewModel> Categorias { get; set; } = new();
        public List<SucursalViewModel> Sucursales { get; set; } = new();
        public string? FiltroNombre { get; set; }
        public string? FiltroCategoriaId { get; set; }
        public string? FiltroSucursalId { get; set; }
        public bool? FiltroSoloStockBajo { get; set; }
        public bool? FiltroSoloActivos { get; set; }
        public int TotalInsumos { get; set; }
        public int InsumosStockBajo { get; set; }
    }
}

