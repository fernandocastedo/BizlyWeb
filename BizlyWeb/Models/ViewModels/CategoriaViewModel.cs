using System.ComponentModel.DataAnnotations;

namespace BizlyWeb.Models.ViewModels
{
    /// <summary>
    /// ViewModel para Categoria - Capa de Presentación
    /// </summary>
    public class CategoriaViewModel
    {
        public string? Id { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}

