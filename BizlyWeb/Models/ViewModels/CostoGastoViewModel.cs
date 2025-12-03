using System.ComponentModel.DataAnnotations;

namespace BizlyWeb.Models.ViewModels
{
    /// <summary>
    /// ViewModel para CostoGasto - Capa de Presentación
    /// </summary>
    public class CostoGastoViewModel
    {
        public string? Id { get; set; }

        [Required(ErrorMessage = "La descripción es requerida")]
        [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El monto es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        [Display(Name = "Monto")]
        public decimal Monto { get; set; }

        [Required(ErrorMessage = "La fecha es requerida")]
        [Display(Name = "Fecha")]
        [DataType(DataType.Date)]
        public DateTime Fecha { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "La categoría financiera es requerida")]
        [Display(Name = "Categoría Financiera")]
        public string CategoriaFinanciera { get; set; } = string.Empty; // "DIRECTO" / "ADMINISTRATIVO"

        [Required(ErrorMessage = "La clasificación es requerida")]
        [Display(Name = "Clasificación")]
        public string Clasificacion { get; set; } = string.Empty; // "FIJO" / "VARIABLE"

        [Display(Name = "Sucursal")]
        public string? SucursalId { get; set; }

        [Display(Name = "Insumo (Opcional)")]
        public string? InsumoId { get; set; }

        [Display(Name = "Trabajador (Opcional)")]
        public string? TrabajadorId { get; set; }

        // Para el formulario
        public List<SucursalViewModel> Sucursales { get; set; } = new();
        public List<InsumoViewModel> Insumos { get; set; } = new();
    }
}

