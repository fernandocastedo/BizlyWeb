using System.ComponentModel.DataAnnotations;

namespace BizlyWeb.Models.ViewModels
{
    /// <summary>
    /// ViewModel para Cliente - Capa de Presentación
    /// </summary>
    public class ClienteViewModel
    {
        public string? Id { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El NIT es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El NIT debe ser un número válido")]
        [Display(Name = "NIT")]
        public int Nit { get; set; }

        [Required(ErrorMessage = "El teléfono es requerido")]
        [StringLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres")]
        [Phone(ErrorMessage = "El formato del teléfono no es válido")]
        [Display(Name = "Teléfono")]
        public string Telefono { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es requerido")]
        [StringLength(200, ErrorMessage = "El email no puede exceder 200 caracteres")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La dirección es requerida")]
        [StringLength(500, ErrorMessage = "La dirección no puede exceder 500 caracteres")]
        [Display(Name = "Dirección")]
        public string Direccion { get; set; } = string.Empty;

        [Display(Name = "Sucursal")]
        public string? SucursalId { get; set; }

        public DateTime CreatedAt { get; set; }

        // Para el formulario
        public List<SucursalViewModel> Sucursales { get; set; } = new();
    }
}
