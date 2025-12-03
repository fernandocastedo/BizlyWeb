using BizlyWeb.Attributes;
using BizlyWeb.Models.DTOs;
using BizlyWeb.Models.ViewModels;
using BizlyWeb.Services;
using Microsoft.AspNetCore.Mvc;

namespace BizlyWeb.Controllers
{
    [AuthorizeRole("EMPRENDEDOR")]
    public class InventarioController : Controller
    {
        private readonly InventarioService _inventarioService;
        private readonly SucursalService _sucursalService;
        private readonly EmpresaService _empresaService;
        private readonly ILogger<InventarioController> _logger;

        public InventarioController(
            InventarioService inventarioService,
            SucursalService sucursalService,
            EmpresaService empresaService,
            ILogger<InventarioController> logger)
        {
            _inventarioService = inventarioService;
            _sucursalService = sucursalService;
            _empresaService = empresaService;
            _logger = logger;
        }

        /// <summary>
        /// Vista principal: Lista de insumos con filtros
        /// </summary>
        public async Task<IActionResult> Index(string? nombre, string? categoriaId, string? sucursalId, bool? soloStockBajo, bool? soloActivos)
        {
            try
            {
                var insumos = await _inventarioService.ObtenerInsumosAsync();
                var categorias = await _inventarioService.ObtenerCategoriasAsync();
                var sucursales = await _sucursalService.ObtenerSucursalesAsync();

                // Aplicar filtros
                if (!string.IsNullOrEmpty(nombre))
                {
                    insumos = insumos.Where(i => i.Nombre.Contains(nombre, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                if (!string.IsNullOrEmpty(categoriaId))
                {
                    insumos = insumos.Where(i => i.CategoriaId == categoriaId).ToList();
                }

                if (!string.IsNullOrEmpty(sucursalId))
                {
                    insumos = insumos.Where(i => i.SucursalId == sucursalId).ToList();
                }

                if (soloStockBajo == true)
                {
                    insumos = insumos.Where(i => i.Activo && i.Cantidad <= i.StockMinimo).ToList();
                }

                if (soloActivos == true)
                {
                    insumos = insumos.Where(i => i.Activo).ToList();
                }

                // Convertir a ViewModels
                var insumosViewModel = insumos.Select(i =>
                {
                    var categoria = categorias.FirstOrDefault(c => c.Id == i.CategoriaId);
                    return new InsumoViewModel
                    {
                        Id = i.Id,
                        CategoriaId = i.CategoriaId,
                        Nombre = i.Nombre,
                        Descripcion = i.Descripcion,
                        Cantidad = i.Cantidad,
                        UnidadMedida = i.UnidadMedida,
                        PrecioUnitario = i.PrecioUnitario,
                        PrecioTotal = i.PrecioTotal,
                        StockMinimo = i.StockMinimo,
                        Activo = i.Activo,
                        StockBajo = i.Activo && i.Cantidad <= i.StockMinimo,
                        CategoriaNombre = categoria?.Nombre
                    };
                }).ToList();

                var viewModel = new InventarioIndexViewModel
                {
                    Insumos = insumosViewModel,
                    Categorias = categorias.Select(c => new CategoriaViewModel
                    {
                        Id = c.Id,
                        Nombre = c.Nombre,
                        Descripcion = c.Descripcion
                    }).ToList(),
                    Sucursales = sucursales.Select(s => new SucursalViewModel
                    {
                        Id = s.Id,
                        Nombre = s.Nombre,
                        Direccion = s.Direccion,
                        Ciudad = s.Ciudad,
                        Departamento = s.Departamento,
                        Telefono = s.Telefono
                    }).ToList(),
                    FiltroNombre = nombre,
                    FiltroCategoriaId = categoriaId,
                    FiltroSucursalId = sucursalId,
                    FiltroSoloStockBajo = soloStockBajo,
                    FiltroSoloActivos = soloActivos,
                    TotalInsumos = insumosViewModel.Count,
                    InsumosStockBajo = insumosViewModel.Count(i => i.StockBajo)
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener inventario");
                TempData["ErrorMessage"] = "Ocurrió un error al cargar el inventario.";
                return View(new InventarioIndexViewModel());
            }
        }

        /// <summary>
        /// Vista para crear un nuevo insumo
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            try
            {
                // Verificar que el usuario tenga empresa asociada
                var empresa = await _empresaService.ObtenerEmpresaActualAsync();
                if (empresa == null)
                {
                    TempData["ErrorMessage"] = "No se pudo obtener la información de tu empresa. Por favor, cierra sesión e inicia sesión nuevamente.";
                    return RedirectToAction("Login", "Auth");
                }

                var categorias = await _inventarioService.ObtenerCategoriasAsync();
                var sucursales = await _sucursalService.ObtenerSucursalesAsync();

                if (!sucursales.Any())
                {
                    TempData["WarningMessage"] = "No tienes sucursales creadas. Por favor, crea al menos una sucursal en la sección de Configuración antes de crear insumos.";
                }

                ViewBag.Categorias = categorias.Select(c => new CategoriaViewModel
                {
                    Id = c.Id,
                    Nombre = c.Nombre,
                    Descripcion = c.Descripcion
                }).ToList();

                ViewBag.Sucursales = sucursales.Select(s => new SucursalViewModel
                {
                    Id = s.Id,
                    Nombre = s.Nombre
                }).ToList();

                return View(new InsumoViewModel());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar vista de crear insumo");
                TempData["ErrorMessage"] = "Ocurrió un error al cargar el formulario.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Procesa la creación de un nuevo insumo
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InsumoViewModel model, string? sucursalId)
        {
            if (!ModelState.IsValid)
            {
                var categorias = await _inventarioService.ObtenerCategoriasAsync();
                var sucursales = await _sucursalService.ObtenerSucursalesAsync();

                ViewBag.Categorias = categorias.Select(c => new CategoriaViewModel
                {
                    Id = c.Id,
                    Nombre = c.Nombre,
                    Descripcion = c.Descripcion
                }).ToList();

                ViewBag.Sucursales = sucursales.Select(s => new SucursalViewModel
                {
                    Id = s.Id,
                    Nombre = s.Nombre
                }).ToList();

                return View(model);
            }

            try
            {
                // Validar que haya sucursales disponibles
                var sucursales = await _sucursalService.ObtenerSucursalesAsync();
                if (!sucursales.Any())
                {
                    TempData["ErrorMessage"] = "No hay sucursales disponibles. Por favor, crea una sucursal primero en la sección de Configuración.";
                    return RedirectToAction(nameof(Create));
                }

                // Si no se especificó sucursal, usar la primera disponible
                if (string.IsNullOrEmpty(sucursalId))
                {
                    sucursalId = sucursales.First().Id;
                }

                var dto = new InsumoDto
                {
                    CategoriaId = string.IsNullOrEmpty(model.CategoriaId) ? null : model.CategoriaId,
                    Nombre = model.Nombre,
                    Descripcion = model.Descripcion,
                    Cantidad = model.Cantidad,
                    UnidadMedida = model.UnidadMedida,
                    PrecioUnitario = model.PrecioUnitario,
                    StockMinimo = model.StockMinimo,
                    Activo = model.Activo,
                    SucursalId = sucursalId ?? string.Empty
                };

                var resultado = await _inventarioService.CrearInsumoAsync(dto);
                if (resultado != null)
                {
                    TempData["SuccessMessage"] = "Insumo creado correctamente.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ErrorMessage"] = "No se pudo crear el insumo.";
                }
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Error de negocio al crear insumo");
                TempData["ErrorMessage"] = ex.Message;
                
                // Si es un error de empresa, redirigir al login
                if (ex.Message.Contains("empresa actual"))
                {
                    TempData["ErrorMessage"] = "No se pudo obtener la información de tu empresa. Por favor, cierra sesión e inicia sesión nuevamente.";
                    return RedirectToAction("Login", "Auth");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear insumo");
                TempData["ErrorMessage"] = $"Error al crear el insumo: {ex.Message}";
            }

            var categorias2 = await _inventarioService.ObtenerCategoriasAsync();
            var sucursales2 = await _sucursalService.ObtenerSucursalesAsync();

            ViewBag.Categorias = categorias2.Select(c => new CategoriaViewModel
            {
                Id = c.Id,
                Nombre = c.Nombre,
                Descripcion = c.Descripcion
            }).ToList();

            ViewBag.Sucursales = sucursales2.Select(s => new SucursalViewModel
            {
                Id = s.Id,
                Nombre = s.Nombre
            }).ToList();

            return View(model);
        }

        /// <summary>
        /// Vista para editar un insumo existente
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "Id de insumo inválido.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var insumo = await _inventarioService.ObtenerInsumoPorIdAsync(id);
                if (insumo == null)
                {
                    TempData["ErrorMessage"] = "Insumo no encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                var categorias = await _inventarioService.ObtenerCategoriasAsync();
                var sucursales = await _sucursalService.ObtenerSucursalesAsync();

                ViewBag.Categorias = categorias.Select(c => new CategoriaViewModel
                {
                    Id = c.Id,
                    Nombre = c.Nombre,
                    Descripcion = c.Descripcion
                }).ToList();

                ViewBag.Sucursales = sucursales.Select(s => new SucursalViewModel
                {
                    Id = s.Id,
                    Nombre = s.Nombre
                }).ToList();

                var viewModel = new InsumoViewModel
                {
                    Id = insumo.Id,
                    CategoriaId = insumo.CategoriaId,
                    Nombre = insumo.Nombre,
                    Descripcion = insumo.Descripcion,
                    Cantidad = insumo.Cantidad,
                    UnidadMedida = insumo.UnidadMedida,
                    PrecioUnitario = insumo.PrecioUnitario,
                    PrecioTotal = insumo.PrecioTotal,
                    StockMinimo = insumo.StockMinimo,
                    Activo = insumo.Activo,
                    StockBajo = insumo.Activo && insumo.Cantidad <= insumo.StockMinimo
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar vista de editar insumo {InsumoId}", id);
                TempData["ErrorMessage"] = "Ocurrió un error al cargar el insumo.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Procesa la actualización de un insumo
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(InsumoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var categorias = await _inventarioService.ObtenerCategoriasAsync();
                var sucursales = await _sucursalService.ObtenerSucursalesAsync();

                ViewBag.Categorias = categorias.Select(c => new CategoriaViewModel
                {
                    Id = c.Id,
                    Nombre = c.Nombre,
                    Descripcion = c.Descripcion
                }).ToList();

                ViewBag.Sucursales = sucursales.Select(s => new SucursalViewModel
                {
                    Id = s.Id,
                    Nombre = s.Nombre
                }).ToList();

                return View(model);
            }

            if (string.IsNullOrEmpty(model.Id))
            {
                TempData["ErrorMessage"] = "Id de insumo inválido.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var insumoExistente = await _inventarioService.ObtenerInsumoPorIdAsync(model.Id);
                if (insumoExistente == null)
                {
                    TempData["ErrorMessage"] = "Insumo no encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                var dto = new InsumoDto
                {
                    Id = model.Id,
                    EmpresaId = insumoExistente.EmpresaId,
                    SucursalId = insumoExistente.SucursalId,
                    CategoriaId = string.IsNullOrEmpty(model.CategoriaId) ? null : model.CategoriaId,
                    Nombre = model.Nombre,
                    Descripcion = model.Descripcion,
                    Cantidad = model.Cantidad,
                    UnidadMedida = model.UnidadMedida,
                    PrecioUnitario = model.PrecioUnitario,
                    StockMinimo = model.StockMinimo,
                    Activo = model.Activo
                };

                var resultado = await _inventarioService.ActualizarInsumoAsync(dto);
                if (resultado)
                {
                    TempData["SuccessMessage"] = "Insumo actualizado correctamente.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ErrorMessage"] = "No se pudo actualizar el insumo.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar insumo {InsumoId}", model.Id);
                TempData["ErrorMessage"] = $"Error al actualizar el insumo: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Vista para actualizar el stock de un insumo
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ActualizarStock(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "Id de insumo inválido.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var insumo = await _inventarioService.ObtenerInsumoPorIdAsync(id);
                if (insumo == null)
                {
                    TempData["ErrorMessage"] = "Insumo no encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new InsumoViewModel
                {
                    Id = insumo.Id,
                    Nombre = insumo.Nombre,
                    Cantidad = insumo.Cantidad,
                    UnidadMedida = insumo.UnidadMedida,
                    StockMinimo = insumo.StockMinimo,
                    StockBajo = insumo.Activo && insumo.Cantidad <= insumo.StockMinimo
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar vista de actualizar stock {InsumoId}", id);
                TempData["ErrorMessage"] = "Ocurrió un error al cargar el insumo.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Procesa la actualización de stock
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarStock(string id, decimal nuevaCantidad, string tipoMovimiento, string motivo)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "Id de insumo inválido.";
                return RedirectToAction(nameof(Index));
            }

            if (nuevaCantidad < 0)
            {
                TempData["ErrorMessage"] = "La cantidad no puede ser negativa.";
                return RedirectToAction(nameof(ActualizarStock), new { id });
            }

            if (string.IsNullOrWhiteSpace(tipoMovimiento))
            {
                tipoMovimiento = "ajuste";
            }

            if (string.IsNullOrWhiteSpace(motivo))
            {
                motivo = $"Actualización de stock - {tipoMovimiento}";
            }

            try
            {
                var resultado = await _inventarioService.ActualizarStockAsync(id, nuevaCantidad, tipoMovimiento, motivo);
                if (resultado)
                {
                    TempData["SuccessMessage"] = "Stock actualizado correctamente.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ErrorMessage"] = "No se pudo actualizar el stock.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar stock {InsumoId}", id);
                TempData["ErrorMessage"] = $"Error al actualizar el stock: {ex.Message}";
            }

            return RedirectToAction(nameof(ActualizarStock), new { id });
        }

        /// <summary>
        /// Vista del historial de movimientos de inventario
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Historial(string? insumoId)
        {
            try
            {
                List<RegistroInventarioDto> registros;

                if (!string.IsNullOrEmpty(insumoId))
                {
                    registros = await _inventarioService.ObtenerRegistrosPorInsumoAsync(insumoId);
                }
                else
                {
                    registros = await _inventarioService.ObtenerRegistrosInventarioAsync();
                }

                // Obtener nombres de insumos para mostrar en la vista
                var insumos = await _inventarioService.ObtenerInsumosAsync();
                var insumosDict = insumos.ToDictionary(i => i.Id!, i => i.Nombre);

                var viewModel = registros.Select(r => new RegistroInventarioViewModel
                {
                    Id = r.Id,
                    InsumoId = r.InsumoId,
                    InsumoNombre = insumosDict.ContainsKey(r.InsumoId) ? insumosDict[r.InsumoId] : "N/A",
                    TipoMovimiento = r.TipoMovimiento,
                    CantidadAnterior = r.CantidadAnterior,
                    CantidadNueva = r.CantidadNueva,
                    Diferencia = r.CantidadNueva - r.CantidadAnterior,
                    Motivo = r.Motivo,
                    UsuarioNombre = "Usuario", // TODO: Obtener nombre del usuario desde la API
                    CreatedAt = r.CreatedAt
                }).OrderByDescending(r => r.CreatedAt).ToList();

                ViewBag.InsumoId = insumoId;
                ViewBag.Insumos = insumos.Select(i => new { i.Id, i.Nombre }).ToList();

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener historial de inventario");
                TempData["ErrorMessage"] = "Ocurrió un error al cargar el historial.";
                return View(new List<RegistroInventarioViewModel>());
            }
        }

        /// <summary>
        /// Elimina un insumo (eliminación lógica)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "Id de insumo inválido.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var resultado = await _inventarioService.EliminarInsumoAsync(id);
                TempData[resultado ? "SuccessMessage" : "ErrorMessage"] =
                    resultado ? "Insumo eliminado correctamente." : "No se pudo eliminar el insumo.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar insumo {InsumoId}", id);
                TempData["ErrorMessage"] = "Ocurrió un error al eliminar el insumo.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

