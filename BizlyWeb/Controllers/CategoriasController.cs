using BizlyWeb.Attributes;
using BizlyWeb.Models.DTOs;
using BizlyWeb.Models.ViewModels;
using BizlyWeb.Services;
using Microsoft.AspNetCore.Mvc;

namespace BizlyWeb.Controllers
{
    [AuthorizeRole("EMPRENDEDOR")]
    public class CategoriasController : Controller
    {
        private readonly CategoriaService _categoriaService;
        private readonly ILogger<CategoriasController> _logger;

        public CategoriasController(
            CategoriaService categoriaService,
            ILogger<CategoriasController> logger)
        {
            _categoriaService = categoriaService;
            _logger = logger;
        }

        /// <summary>
        /// Vista principal: Lista de categorías
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                var categorias = await _categoriaService.ObtenerCategoriasAsync();
                var viewModel = categorias.Select(c => new CategoriaViewModel
                {
                    Id = c.Id,
                    Nombre = c.Nombre,
                    Descripcion = c.Descripcion,
                    CreatedAt = c.CreatedAt
                }).ToList();

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener categorías");
                TempData["Error"] = "Error al cargar la lista de categorías. Por favor, intente nuevamente.";
                return View(new List<CategoriaViewModel>());
            }
        }

        /// <summary>
        /// Vista para crear una nueva categoría
        /// </summary>
        [HttpGet]
        public IActionResult Create()
        {
            return View(new CategoriaViewModel());
        }

        /// <summary>
        /// Procesa la creación de una nueva categoría
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoriaViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            try
            {
                var categoriaDto = new CategoriaDto
                {
                    Nombre = viewModel.Nombre,
                    Descripcion = viewModel.Descripcion ?? string.Empty
                };

                var resultado = await _categoriaService.CrearCategoriaAsync(categoriaDto);
                if (resultado != null)
                {
                    TempData["Success"] = "Categoría creada exitosamente.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["Error"] = "No se pudo crear la categoría. Por favor, intente nuevamente.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear categoría");
                TempData["Error"] = $"Error al crear la categoría: {ex.Message}";
            }

            return View(viewModel);
        }

        /// <summary>
        /// Vista para editar una categoría existente
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "ID de categoría no válido.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var categoria = await _categoriaService.ObtenerCategoriaPorIdAsync(id);
                if (categoria == null)
                {
                    TempData["Error"] = "Categoría no encontrada.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new CategoriaViewModel
                {
                    Id = categoria.Id,
                    Nombre = categoria.Nombre,
                    Descripcion = categoria.Descripcion,
                    CreatedAt = categoria.CreatedAt
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar categoría para edición");
                TempData["Error"] = "Error al cargar la categoría. Por favor, intente nuevamente.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Procesa la actualización de una categoría
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, CategoriaViewModel viewModel)
        {
            if (string.IsNullOrEmpty(id) || id != viewModel.Id)
            {
                TempData["Error"] = "ID de categoría no válido.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            try
            {
                var categoriaDto = new CategoriaDto
                {
                    Id = viewModel.Id,
                    Nombre = viewModel.Nombre,
                    Descripcion = viewModel.Descripcion ?? string.Empty,
                    CreatedAt = viewModel.CreatedAt
                };

                var resultado = await _categoriaService.ActualizarCategoriaAsync(id, categoriaDto);
                if (resultado)
                {
                    TempData["Success"] = "Categoría actualizada exitosamente.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["Error"] = "No se pudo actualizar la categoría. Por favor, intente nuevamente.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar categoría");
                TempData["Error"] = $"Error al actualizar la categoría: {ex.Message}";
            }

            return View(viewModel);
        }

        /// <summary>
        /// Elimina una categoría
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "ID de categoría no válido.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var resultado = await _categoriaService.EliminarCategoriaAsync(id);
                if (resultado)
                {
                    TempData["Success"] = "Categoría eliminada exitosamente.";
                }
                else
                {
                    TempData["Error"] = "No se pudo eliminar la categoría. Por favor, intente nuevamente.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar categoría");
                TempData["Error"] = $"Error al eliminar la categoría: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}


