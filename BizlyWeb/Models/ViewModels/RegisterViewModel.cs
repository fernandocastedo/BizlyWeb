using System.ComponentModel.DataAnnotations;

namespace BizlyWeb.Models.ViewModels
{
    /// <summary>
    /// ViewModel para la vista de Registro (dos pasos)
    /// </summary>
    public class RegisterViewModel
    {
        // Paso 1: Datos del Emprendimiento
        [Required(ErrorMessage = "El nombre del emprendimiento es requerido")]
        [StringLength(100, ErrorMessage = "El nombre debe tener máximo 100 caracteres")]
        [Display(Name = "Nombre del Emprendimiento")]
        public string NombreEmpresa { get; set; } = string.Empty;

        [Required(ErrorMessage = "El rubro es requerido")]
        [StringLength(100, ErrorMessage = "El rubro debe tener máximo 100 caracteres")]
        [Display(Name = "Rubro o Categoría")]
        public string Rubro { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "La descripción debe tener máximo 500 caracteres")]
        [Display(Name = "Descripción (Opcional)")]
        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "El margen de ganancia es requerido")]
        [Range(0, 1000, ErrorMessage = "El margen de ganancia debe estar entre 0 y 1000")]
        [Display(Name = "Margen de Ganancia (%)")]
        public decimal MargenGanancia { get; set; } = 30;

        [Display(Name = "Logo (Opcional)")]
        public IFormFile? Logo { get; set; }

        // Paso 2: Datos del Usuario
        [Required(ErrorMessage = "Tu nombre completo es requerido")]
        [StringLength(100, ErrorMessage = "El nombre debe tener máximo 100 caracteres")]
        [Display(Name = "Nombre Completo")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [StringLength(100, ErrorMessage = "La contraseña debe tener al menos {2} caracteres", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "La confirmación de contraseña es requerida")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Contraseña")]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmPassword { get; set; } = string.Empty;

        // Control del paso actual
        public int PasoActual { get; set; } = 1;
    }
}
