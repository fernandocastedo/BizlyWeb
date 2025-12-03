using BizlyWeb.Attributes;
using BizlyWeb.Models.DTOs;
using BizlyWeb.Models.ViewModels;
using BizlyWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace BizlyWeb.Controllers
{
    [AuthorizeRole("EMPRENDEDOR")]
    public class TrabajadoresController : Controller
    {
        private readonly TrabajadorService _trabajadorService;
        private readonly SucursalService _sucursalService;
        private readonly ApiService _apiService;
        private readonly ILogger<TrabajadoresController> _logger;

        public TrabajadoresController(
            TrabajadorService trabajadorService,
            SucursalService sucursalService,
            ApiService apiService,
            ILogger<TrabajadoresController> logger)
        {
            _trabajadorService = trabajadorService;
            _sucursalService = sucursalService;
            _apiService = apiService;
            _logger = logger;
        }

        /// <summary>
        /// Vista principal: Lista de trabajadores (RF-46)
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                var trabajadores = await _trabajadorService.ObtenerTrabajadoresAsync();
                
                // Obtener información de usuarios asociados
                var usuarios = await _apiService.GetAsync<List<UsuarioApiDto>>("/api/usuarios") ?? new List<UsuarioApiDto>();
                var httpContextAccessor = HttpContext.RequestServices.GetRequiredService<IHttpContextAccessor>();
                var empresaId = SessionHelper.GetEmpresaId(httpContextAccessor);

                var viewModel = new TrabajadorIndexViewModel
                {
                    Trabajadores = trabajadores.Select(t => 
                    {
                        var usuario = usuarios.FirstOrDefault(u => u.TrabajadorId == t.Id && u.EmpresaId == empresaId);
                        return new TrabajadorViewModel
                        {
                            Id = t.Id,
                            Nombre = t.Nombre,
                            Cargo = t.Cargo,
                            SueldoMensual = t.SueldoMensual,
                            TipoGasto = t.TipoGasto,
                            SucursalId = t.SucursalId,
                            TieneUsuario = usuario != null,
                            UsuarioActivo = usuario?.Activo ?? false
                        };
                    }).ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener trabajadores");
                TempData["Error"] = "Error al cargar la lista de trabajadores. Por favor, intente nuevamente.";
                return View(new TrabajadorIndexViewModel());
            }
        }

        /// <summary>
        /// Vista para crear un nuevo trabajador (RF-46)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            try
            {
                var sucursales = await _sucursalService.ObtenerSucursalesAsync();

                var viewModel = new TrabajadorViewModel
                {
                    Sucursales = sucursales.Select(s => new SucursalViewModel
                    {
                        Id = s.Id,
                        Nombre = s.Nombre,
                        Direccion = s.Direccion,
                        Ciudad = s.Ciudad,
                        Departamento = s.Departamento
                    }).ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar formulario de creación de trabajador");
                TempData["Error"] = "Error al cargar el formulario. Por favor, intente nuevamente.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Procesa la creación de un nuevo trabajador (RF-46, RF-48)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TrabajadorViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                var sucursales = await _sucursalService.ObtenerSucursalesAsync();
                viewModel.Sucursales = sucursales.Select(s => new SucursalViewModel
                {
                    Id = s.Id,
                    Nombre = s.Nombre,
                    Direccion = s.Direccion,
                    Ciudad = s.Ciudad,
                    Departamento = s.Departamento
                }).ToList();
                return View(viewModel);
            }

            try
            {
                // Crear el trabajador
                var trabajadorDto = new TrabajadorDto
                {
                    Nombre = viewModel.Nombre,
                    Cargo = viewModel.Cargo,
                    SueldoMensual = viewModel.SueldoMensual,
                    TipoGasto = viewModel.TipoGasto,
                    SucursalId = string.IsNullOrEmpty(viewModel.SucursalId) ? null : viewModel.SucursalId
                };

                var trabajadorCreado = await _trabajadorService.CrearTrabajadorAsync(trabajadorDto);
                if (trabajadorCreado == null || string.IsNullOrEmpty(trabajadorCreado.Id))
                {
                    TempData["Error"] = "No se pudo crear el trabajador. Por favor, intente nuevamente.";
                    var sucursales = await _sucursalService.ObtenerSucursalesAsync();
                    viewModel.Sucursales = sucursales.Select(s => new SucursalViewModel
                    {
                        Id = s.Id,
                        Nombre = s.Nombre,
                        Direccion = s.Direccion,
                        Ciudad = s.Ciudad,
                        Departamento = s.Departamento
                    }).ToList();
                    return View(viewModel);
                }

                // Si se solicitó crear usuario, crearlo (RF-48)
                if (viewModel.CrearUsuario && !string.IsNullOrEmpty(viewModel.EmailUsuario) && !string.IsNullOrEmpty(viewModel.PasswordUsuario))
                {
                    var usuarioCreado = await _trabajadorService.CrearUsuarioTrabajadorAsync(
                        trabajadorCreado.Id,
                        viewModel.Nombre,
                        viewModel.EmailUsuario,
                        viewModel.PasswordUsuario,
                        trabajadorCreado.SucursalId);

                    if (usuarioCreado)
                    {
                        TempData["Success"] = "Trabajador y usuario creados exitosamente.";
                    }
                    else
                    {
                        TempData["Warning"] = "Trabajador creado, pero no se pudo crear el usuario. Puedes crearlo más tarde desde la edición.";
                    }
                }
                else
                {
                    TempData["Success"] = "Trabajador creado exitosamente.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear trabajador");
                TempData["Error"] = $"Error al crear el trabajador: {ex.Message}";
            }

            var sucursalesRetry = await _sucursalService.ObtenerSucursalesAsync();
            viewModel.Sucursales = sucursalesRetry.Select(s => new SucursalViewModel
            {
                Id = s.Id,
                Nombre = s.Nombre,
                Direccion = s.Direccion,
                Ciudad = s.Ciudad,
                Departamento = s.Departamento
            }).ToList();
            return View(viewModel);
        }

        /// <summary>
        /// Vista para editar un trabajador existente (RF-46)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "ID de trabajador no válido.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var trabajador = await _trabajadorService.ObtenerTrabajadorPorIdAsync(id);
                if (trabajador == null)
                {
                    TempData["Error"] = "Trabajador no encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                var sucursales = await _sucursalService.ObtenerSucursalesAsync();
                
                // Verificar si tiene usuario asociado
                var usuarios = await _apiService.GetAsync<List<UsuarioApiDto>>("/api/usuarios") ?? new List<UsuarioApiDto>();
                var httpContextAccessor = HttpContext.RequestServices.GetRequiredService<IHttpContextAccessor>();
                var empresaId = SessionHelper.GetEmpresaId(httpContextAccessor);
                var usuario = usuarios.FirstOrDefault(u => u.TrabajadorId == id && u.EmpresaId == empresaId);

                var viewModel = new TrabajadorViewModel
                {
                    Id = trabajador.Id,
                    Nombre = trabajador.Nombre,
                    Cargo = trabajador.Cargo,
                    SueldoMensual = trabajador.SueldoMensual,
                    TipoGasto = trabajador.TipoGasto,
                    SucursalId = trabajador.SucursalId,
                    Sucursales = sucursales.Select(s => new SucursalViewModel
                    {
                        Id = s.Id,
                        Nombre = s.Nombre,
                        Direccion = s.Direccion,
                        Ciudad = s.Ciudad,
                        Departamento = s.Departamento
                    }).ToList(),
                    TieneUsuario = usuario != null,
                    UsuarioActivo = usuario?.Activo ?? false
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar trabajador para edición");
                TempData["Error"] = "Error al cargar el trabajador. Por favor, intente nuevamente.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Procesa la actualización de un trabajador (RF-46)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, TrabajadorViewModel viewModel)
        {
            if (string.IsNullOrEmpty(id) || id != viewModel.Id)
            {
                TempData["Error"] = "ID de trabajador no válido.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                var sucursales = await _sucursalService.ObtenerSucursalesAsync();
                viewModel.Sucursales = sucursales.Select(s => new SucursalViewModel
                {
                    Id = s.Id,
                    Nombre = s.Nombre,
                    Direccion = s.Direccion,
                    Ciudad = s.Ciudad,
                    Departamento = s.Departamento
                }).ToList();
                return View(viewModel);
            }

            try
            {
                var trabajadorDto = new TrabajadorDto
                {
                    Id = viewModel.Id,
                    Nombre = viewModel.Nombre,
                    Cargo = viewModel.Cargo,
                    SueldoMensual = viewModel.SueldoMensual,
                    TipoGasto = viewModel.TipoGasto,
                    SucursalId = string.IsNullOrEmpty(viewModel.SucursalId) ? null : viewModel.SucursalId
                };

                var resultado = await _trabajadorService.ActualizarTrabajadorAsync(id, trabajadorDto);
                if (resultado)
                {
                    // Si se solicitó crear usuario y no tiene uno, crearlo (RF-48)
                    if (viewModel.CrearUsuario && !viewModel.TieneUsuario && 
                        !string.IsNullOrEmpty(viewModel.EmailUsuario) && !string.IsNullOrEmpty(viewModel.PasswordUsuario))
                    {
                        var usuarioCreado = await _trabajadorService.CrearUsuarioTrabajadorAsync(
                            id,
                            viewModel.Nombre,
                            viewModel.EmailUsuario,
                            viewModel.PasswordUsuario,
                            trabajadorDto.SucursalId);

                        if (usuarioCreado)
                        {
                            TempData["Success"] = "Trabajador actualizado y usuario creado exitosamente.";
                        }
                        else
                        {
                            TempData["Warning"] = "Trabajador actualizado, pero no se pudo crear el usuario.";
                        }
                    }
                    else
                    {
                        TempData["Success"] = "Trabajador actualizado exitosamente.";
                    }
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["Error"] = "No se pudo actualizar el trabajador. Por favor, intente nuevamente.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar trabajador");
                TempData["Error"] = $"Error al actualizar el trabajador: {ex.Message}";
            }

            var sucursalesRetry = await _sucursalService.ObtenerSucursalesAsync();
            viewModel.Sucursales = sucursalesRetry.Select(s => new SucursalViewModel
            {
                Id = s.Id,
                Nombre = s.Nombre,
                Direccion = s.Direccion,
                Ciudad = s.Ciudad,
                Departamento = s.Departamento
            }).ToList();
            return View(viewModel);
        }

        /// <summary>
        /// Elimina un trabajador
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "ID de trabajador no válido.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var resultado = await _trabajadorService.EliminarTrabajadorAsync(id);
                if (resultado)
                {
                    TempData["Success"] = "Trabajador eliminado exitosamente.";
                }
                else
                {
                    TempData["Error"] = "No se pudo eliminar el trabajador. Por favor, intente nuevamente.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar trabajador");
                TempData["Error"] = $"Error al eliminar el trabajador: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Desactiva el acceso de un trabajador (RF-52)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DesactivarAcceso(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "ID de trabajador no válido.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var resultado = await _trabajadorService.DesactivarAccesoTrabajadorAsync(id);
                if (resultado)
                {
                    TempData["Success"] = "Acceso del trabajador desactivado exitosamente.";
                }
                else
                {
                    TempData["Error"] = "No se pudo desactivar el acceso. El trabajador puede no tener un usuario asociado.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al desactivar acceso del trabajador");
                TempData["Error"] = $"Error al desactivar el acceso: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Vista de reporte de desempeño de un trabajador (RF-51)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Desempeno(string id, DateTime? fechaInicio, DateTime? fechaFin)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "ID de trabajador no válido.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var trabajador = await _trabajadorService.ObtenerTrabajadorPorIdAsync(id);
                if (trabajador == null)
                {
                    TempData["Error"] = "Trabajador no encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                var desempeno = await _trabajadorService.ObtenerDesempenoTrabajadorAsync(id, fechaInicio, fechaFin);
                
                var viewModel = new DesempenoTrabajadorViewModel
                {
                    TrabajadorId = trabajador.Id ?? string.Empty,
                    TrabajadorNombre = trabajador.Nombre,
                    Cargo = trabajador.Cargo,
                    TotalVentas = desempeno?.TotalVentas ?? 0,
                    TotalIngresos = desempeno?.TotalIngresos ?? 0,
                    PromedioVenta = desempeno?.PromedioVenta ?? 0,
                    VentasMesActual = desempeno?.VentasMesActual ?? 0,
                    IngresosMesActual = desempeno?.IngresosMesActual ?? 0,
                    FechaInicio = fechaInicio,
                    FechaFin = fechaFin
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener desempeño del trabajador");
                TempData["Error"] = "Error al cargar el reporte de desempeño. Por favor, intente nuevamente.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}

