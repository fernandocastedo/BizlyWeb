using System.ComponentModel.DataAnnotations;

namespace BizlyWeb.Models.ViewModels
{
    public class ConfiguracionViewModel
    {
        public EmpresaViewModel Empresa { get; set; } = new();
        public List<SucursalViewModel> Sucursales { get; set; } = new();

        // Formulario de sucursal
        // Nota: No usar [Required] aquí porque causa conflictos con el formulario de ActualizarEmpresa
        // La validación se hace manualmente en CrearSucursal
        [Display(Name = "Nombre de la Sucursal")]
        public string NuevaSucursalNombre { get; set; } = string.Empty;

        [Display(Name = "Dirección")]
        public string NuevaSucursalDireccion { get; set; } = string.Empty;

        [Display(Name = "Ciudad")]
        public string NuevaSucursalCiudad { get; set; } = string.Empty;

        [Display(Name = "Departamento")]
        public string NuevaSucursalDepartamento { get; set; } = string.Empty;

        [Display(Name = "Teléfono")]
        public string NuevaSucursalTelefono { get; set; } = string.Empty;
    }
}

