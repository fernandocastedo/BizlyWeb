using BizlyWeb.Models.DTOs;
using BizlyWeb.Models.ViewModels;
using BizlyWeb.Services;
using Microsoft.AspNetCore.Mvc;

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
                    Telefono = s.Telefono
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarEmpresa(EmpresaViewModel model, IFormFile? logo)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Revisa los datos del formulario.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                string? logoUrl = model.LogoUrl;
                if (logo != null && logo.Length > 0)
                {
                    logoUrl = await _fileService.SaveLogoAsync(logo);
                }

                var dto = new EmpresaDto
                {
                    Id = model.Id,
                    Nombre = model.Nombre,
                    Rubro = model.Rubro,
                    Descripcion = model.Descripcion,
                    MargenGanancia = model.MargenGanancia,
                    LogoUrl = logoUrl ?? string.Empty
                };

                var result = await _empresaService.ActualizarEmpresaAsync(dto);
                TempData[result ? "SuccessMessage" : "ErrorMessage"] =
                    result ? "Datos actualizados correctamente." : "No se pudo actualizar la empresa.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar empresa");
                TempData["ErrorMessage"] = "Ocurrió un error al actualizar la empresa.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearSucursal(ConfiguracionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Revisa los datos de la nueva sucursal.";
                return RedirectToAction(nameof(Index));
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
                    Telefono = model.NuevaSucursalTelefono ?? string.Empty
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

