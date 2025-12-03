using System.ComponentModel.DataAnnotations;

namespace BizlyWeb.Models.ViewModels
{
    public class ConfiguracionViewModel
    {
        public EmpresaViewModel Empresa { get; set; } = new();
        public List<SucursalViewModel> Sucursales { get; set; } = new();

        // Formulario de sucursal
        [Display(Name = "Nombre de la Sucursal")]
        [Required(ErrorMessage = "El nombre de la sucursal es requerido")]
        public string NuevaSucursalNombre { get; set; } = string.Empty;

        [Display(Name = "Dirección")]
        [Required(ErrorMessage = "La dirección es requerida")]
        public string NuevaSucursalDireccion { get; set; } = string.Empty;

        [Display(Name = "Ciudad")]
        public string NuevaSucursalCiudad { get; set; } = string.Empty;

        [Display(Name = "Departamento")]
        public string NuevaSucursalDepartamento { get; set; } = string.Empty;

        [Display(Name = "Teléfono")]
        public string NuevaSucursalTelefono { get; set; } = string.Empty;
    }
}

