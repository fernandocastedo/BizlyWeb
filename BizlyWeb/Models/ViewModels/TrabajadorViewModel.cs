using System.ComponentModel.DataAnnotations;

namespace BizlyWeb.Models.ViewModels
{
    /// <summary>
    /// ViewModel para Trabajador - Capa de Presentación
    /// </summary>
    public class TrabajadorViewModel
    {
        public string? Id { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El cargo es requerido")]
        [StringLength(100, ErrorMessage = "El cargo no puede exceder 100 caracteres")]
        [Display(Name = "Cargo")]
        public string Cargo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El sueldo mensual es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El sueldo debe ser mayor a 0")]
        [Display(Name = "Sueldo Mensual")]
        public decimal SueldoMensual { get; set; }

        [Required(ErrorMessage = "El tipo de gasto es requerido")]
        [Display(Name = "Tipo de Gasto")]
        public string TipoGasto { get; set; } = string.Empty; // "fijo" / "variable"

        [Display(Name = "Sucursal")]
        public string? SucursalId { get; set; }

        // Para el formulario
        public List<SucursalViewModel> Sucursales { get; set; } = new();

        // Para creación de usuario (RF-48)
        [Display(Name = "Crear Usuario")]
        public bool CrearUsuario { get; set; } = false;

        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        [Display(Name = "Email")]
        public string? EmailUsuario { get; set; }

        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        [Display(Name = "Contraseña")]
        public string? PasswordUsuario { get; set; }

        // Información del usuario asociado (si existe)
        public bool TieneUsuario { get; set; }
        public bool UsuarioActivo { get; set; }
    }
}


