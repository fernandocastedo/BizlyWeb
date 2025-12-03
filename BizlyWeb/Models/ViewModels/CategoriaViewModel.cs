namespace BizlyWeb.Models.ViewModels
{
    /// <summary>
    /// ViewModel para Categoria - Capa de Datos
    /// </summary>
    public class CategoriaViewModel
    {
        public string? Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }
}

