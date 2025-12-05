using BizlyWeb.Models.DTOs;
using BizlyWeb.Models.ViewModels;
using BizlyWeb.Services;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace BizlyWeb.Controllers
{
    public class ConfiguracionController : Controller
    {
        private readonly EmpresaService _empresaService;
        private readonly SucursalService _sucursalService;
        private readonly FileService _fileService;
        private readonly ILogger<ConfiguracionController> _logger;

        public ConfiguracionController(
            EmpresaService empresaService,
            SucursalService sucursalService,
            FileService fileService,
            ILogger<ConfiguracionController> logger)
        {
            _empresaService = empresaService;
            _sucursalService = sucursalService;
            _fileService = fileService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var empresa = await _empresaService.ObtenerEmpresaActualAsync();
            if (empresa == null)
            {
                TempData["ErrorMessage"] = "No se pudo obtener la información del emprendimiento. Vuelve a iniciar sesión.";
                return RedirectToAction("Login", "Auth");
            }

            var sucursales = await _sucursalService.ObtenerSucursalesAsync();
            var viewModel = new ConfiguracionViewModel
            {
                Empresa = new EmpresaViewModel
                {
                    Id = empresa.Id,
                    Nombre = empresa.Nombre,
                    Rubro = empresa.Rubro,
                    Descripcion = empresa.Descripcion,
                    MargenGanancia = empresa.MargenGanancia,
                    LogoUrl = string.IsNullOrWhiteSpace(empresa.LogoUrl) ? "/img/placeholder-logo.png" : empresa.LogoUrl
                },
                Sucursales = sucursales.Select(s => new SucursalViewModel
                {
                    Id = s.Id,
                    Nombre = s.Nombre,
                    Direccion = s.Direccion,
                    Ciudad = s.Ciudad,
                    Departamento = s.Departamento,
                    Telefono = s.Telefono,
                    Latitud = s.Latitud,
                    Longitud = s.Longitud
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(20971520)] // 20MB
        [RequestFormLimits(MultipartBodyLengthLimit = 20971520, ValueLengthLimit = 20971520)]
        public async Task<IActionResult> ActualizarEmpresa(ConfiguracionViewModel model, IFormFile? logo)
        {
            if (model?.Empresa == null)
            {
                TempData["ErrorMessage"] = "No se recibieron los datos del formulario.";
                return RedirectToAction(nameof(Index));
            }

            // Remover errores de validación relacionados con campos de sucursal
            // porque este formulario solo actualiza la empresa
            ModelState.Remove(nameof(model.NuevaSucursalNombre));
            ModelState.Remove(nameof(model.NuevaSucursalDireccion));
            ModelState.Remove(nameof(model.NuevaSucursalCiudad));
            ModelState.Remove(nameof(model.NuevaSucursalDepartamento));
            ModelState.Remove(nameof(model.NuevaSucursalTelefono));
            ModelState.Remove(nameof(model.NuevaSucursalLatitud));
            ModelState.Remove(nameof(model.NuevaSucursalLongitud));

            // Validar campos requeridos de empresa manualmente
            if (string.IsNullOrWhiteSpace(model.Empresa.Nombre))
            {
                ModelState.AddModelError("Empresa.Nombre", "El nombre es requerido.");
            }
            if (string.IsNullOrWhiteSpace(model.Empresa.Rubro))
            {
                ModelState.AddModelError("Empresa.Rubro", "El rubro es requerido.");
            }
            if (model.Empresa.MargenGanancia < 0)
            {
                ModelState.AddModelError("Empresa.MargenGanancia", "El margen de ganancia debe ser mayor o igual a 0.");
            }

            // Validar solo los campos de empresa
            var empresaErrors = ModelState
                .Where(x => x.Key.StartsWith("Empresa."))
                .SelectMany(x => x.Value?.Errors ?? Enumerable.Empty<Microsoft.AspNetCore.Mvc.ModelBinding.ModelError>())
                .Any();

            if (empresaErrors || !ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Revisa los datos del formulario.";
                // Recargar datos para mostrar el formulario con errores
                var empresa = await _empresaService.ObtenerEmpresaActualAsync();
                var sucursales = await _sucursalService.ObtenerSucursalesAsync();
                if (empresa != null)
                {
                    model.Empresa.Id = empresa.Id;
                    model.Empresa.LogoUrl = string.IsNullOrWhiteSpace(empresa.LogoUrl) ? "/img/placeholder-logo.svg" : empresa.LogoUrl;
                }
                model.Sucursales = sucursales.Select(s => new SucursalViewModel
                {
                    Id = s.Id,
                    Nombre = s.Nombre,
                    Direccion = s.Direccion,
                    Ciudad = s.Ciudad,
                    Departamento = s.Departamento,
                    Telefono = s.Telefono,
                    Latitud = s.Latitud,
                    Longitud = s.Longitud
                }).ToList();
                return View("Index", model);
            }

            try
            {
                // Obtener la empresa actual para preservar CreatedAt y UpdatedAt
                var empresaActual = await _empresaService.ObtenerEmpresaActualAsync();
                if (empresaActual == null)
                {
                    TempData["ErrorMessage"] = "No se pudo obtener la información del emprendimiento.";
                    return RedirectToAction(nameof(Index));
                }

                string? logoUrl = empresaActual.LogoUrl; // Mantener el logo actual por defecto
                if (logo != null && logo.Length > 0)
                {
                    logoUrl = await _fileService.SaveLogoAsync(logo);
                }

                var dto = new EmpresaDto
                {
                    Id = model.Empresa.Id ?? empresaActual.Id,
                    Nombre = model.Empresa.Nombre,
                    Rubro = model.Empresa.Rubro,
                    Descripcion = model.Empresa.Descripcion ?? string.Empty,
                    MargenGanancia = model.Empresa.MargenGanancia,
                    LogoUrl = logoUrl ?? string.Empty,
                    CreatedAt = empresaActual.CreatedAt, // Preservar fecha de creación
                    UpdatedAt = DateTime.UtcNow // Actualizar fecha de modificación
                };

                var result = await _empresaService.ActualizarEmpresaAsync(dto);
                if (result)
                {
                    TempData["SuccessMessage"] = "Datos actualizados correctamente.";
                }
                else
                {
                    TempData["ErrorMessage"] = "No se pudo actualizar la empresa. Verifica los datos e intenta nuevamente.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar empresa: {Message}", ex.Message);
                TempData["ErrorMessage"] = $"Ocurrió un error al actualizar la empresa: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearSucursal(ConfiguracionViewModel model)
        {
            // Remover errores de validación relacionados con campos de empresa
            // porque este formulario solo crea sucursales
            ModelState.Remove(nameof(model.Empresa));

            // Validar campos requeridos de sucursal manualmente
            if (string.IsNullOrWhiteSpace(model.NuevaSucursalNombre))
            {
                ModelState.AddModelError(nameof(model.NuevaSucursalNombre), "El nombre de la sucursal es requerido.");
            }
            if (string.IsNullOrWhiteSpace(model.NuevaSucursalDireccion))
            {
                ModelState.AddModelError(nameof(model.NuevaSucursalDireccion), "La dirección es requerida.");
            }

            // Validar solo los campos de sucursal
            var sucursalErrors = ModelState
                .Where(x => x.Key.StartsWith("NuevaSucursal"))
                .SelectMany(x => x.Value?.Errors ?? Enumerable.Empty<Microsoft.AspNetCore.Mvc.ModelBinding.ModelError>())
                .Any();

            if (sucursalErrors || !ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Revisa los datos de la nueva sucursal.";
                // Recargar datos para mostrar el formulario con errores
                var empresa = await _empresaService.ObtenerEmpresaActualAsync();
                var sucursales = await _sucursalService.ObtenerSucursalesAsync();
                if (empresa != null)
                {
                    model.Empresa = new EmpresaViewModel
                    {
                        Id = empresa.Id,
                        Nombre = empresa.Nombre,
                        Rubro = empresa.Rubro,
                        Descripcion = empresa.Descripcion,
                        MargenGanancia = empresa.MargenGanancia,
                        LogoUrl = string.IsNullOrWhiteSpace(empresa.LogoUrl) ? "/img/placeholder-logo.svg" : empresa.LogoUrl
                    };
                }
                model.Sucursales = sucursales.Select(s => new SucursalViewModel
                {
                    Id = s.Id,
                    Nombre = s.Nombre,
                    Direccion = s.Direccion,
                    Ciudad = s.Ciudad,
                    Departamento = s.Departamento,
                    Telefono = s.Telefono,
                    Latitud = s.Latitud,
                    Longitud = s.Longitud
                }).ToList();
                return View("Index", model);
            }

            try
            {
                var empresa = await _empresaService.ObtenerEmpresaActualAsync();
                if (empresa == null)
                {
                    TempData["ErrorMessage"] = "No se pudo obtener la empresa actual.";
                    return RedirectToAction(nameof(Index));
                }

                var dto = new SucursalDto
                {
                    EmpresaId = empresa.Id!,
                    Nombre = model.NuevaSucursalNombre,
                    Direccion = model.NuevaSucursalDireccion,
                    Ciudad = model.NuevaSucursalCiudad ?? string.Empty,
                    Departamento = model.NuevaSucursalDepartamento ?? string.Empty,
                    Telefono = model.NuevaSucursalTelefono ?? string.Empty,
                    Latitud = model.NuevaSucursalLatitud,
                    Longitud = model.NuevaSucursalLongitud
                };

                var result = await _sucursalService.CrearSucursalAsync(dto);
                TempData[result ? "SuccessMessage" : "ErrorMessage"] =
                    result ? "Sucursal creada correctamente." : "No se pudo crear la sucursal.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear sucursal");
                TempData["ErrorMessage"] = "Ocurrió un error al crear la sucursal.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarSucursal(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "Id de sucursal inválido.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var resultado = await _sucursalService.EliminarSucursalAsync(id);
                TempData[resultado ? "SuccessMessage" : "ErrorMessage"] =
                    resultado ? "Sucursal eliminada." : "No se pudo eliminar la sucursal.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar sucursal {SucursalId}", id);
                TempData["ErrorMessage"] = "Ocurrió un error al eliminar la sucursal.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

