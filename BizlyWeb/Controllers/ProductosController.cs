using BizlyWeb.Attributes;
using BizlyWeb.Models.DTOs;
using BizlyWeb.Models.ViewModels;
using BizlyWeb.Services;
using Microsoft.AspNetCore.Mvc;

namespace BizlyWeb.Controllers
{
    [AuthorizeRole("EMPRENDEDOR")]
    public class ProductosController : Controller
    {
        private readonly ProductoService _productoService;
        private readonly InventarioService _inventarioService;
        private readonly EmpresaService _empresaService;
        private readonly ILogger<ProductosController> _logger;

        public ProductosController(
            ProductoService productoService,
            InventarioService inventarioService,
            EmpresaService empresaService,
            ILogger<ProductosController> logger)
        {
            _productoService = productoService;
            _inventarioService = inventarioService;
            _empresaService = empresaService;
            _logger = logger;
        }

        /// <summary>
        /// Vista principal: Lista de productos con filtros
        /// </summary>
        public async Task<IActionResult> Index(string? nombre, string? categoriaId, bool? soloActivos)
        {
            try
            {
                var productos = await _productoService.ObtenerProductosAsync();
                var categorias = await _inventarioService.ObtenerCategoriasAsync();

                // Aplicar filtros
                if (!string.IsNullOrEmpty(nombre))
                {
                    productos = productos.Where(p => p.Nombre.Contains(nombre, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                if (!string.IsNullOrEmpty(categoriaId))
                {
                    productos = productos.Where(p => p.CategoriaId == categoriaId).ToList();
                }

                if (soloActivos == true)
                {
                    productos = productos.Where(p => p.Activo).ToList();
                }

                // Convertir a ViewModels y calcular precios sugeridos
                var productosViewModel = new List<ProductoVentaViewModel>();
                foreach (var producto in productos)
                {
                    var insumosProducto = await _productoService.ObtenerInsumosDelProductoAsync(producto.Id!);
                    var precioSugerido = await _productoService.CalcularPrecioSugeridoAsync(producto.Id!);
                    var categoria = categorias.FirstOrDefault(c => c.Id == producto.CategoriaId);

                    productosViewModel.Add(new ProductoVentaViewModel
                    {
                        Id = producto.Id,
                        CategoriaId = producto.CategoriaId,
                        Nombre = producto.Nombre,
                        Descripcion = producto.Descripcion,
                        PrecioVenta = producto.PrecioVenta,
                        PrecioSugerido = precioSugerido,
                        Activo = producto.Activo,
                        CategoriaNombre = categoria?.Nombre
                    });
                }

                var viewModel = new ProductoIndexViewModel
                {
                    Productos = productosViewModel,
                    Categorias = categorias.Select(c => new CategoriaViewModel
                    {
                        Id = c.Id,
                        Nombre = c.Nombre,
                        Descripcion = c.Descripcion
                    }).ToList(),
                    FiltroNombre = nombre,
                    FiltroCategoriaId = categoriaId,
                    FiltroSoloActivos = soloActivos,
                    TotalProductos = productosViewModel.Count
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos");
                TempData["ErrorMessage"] = "Ocurrió un error al cargar los productos.";
                return View(new ProductoIndexViewModel());
            }
        }

        /// <summary>
        /// Vista para crear un nuevo producto
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            try
            {
                var categorias = await _inventarioService.ObtenerCategoriasAsync();
                var insumos = await _inventarioService.ObtenerInsumosAsync();
                var empresa = await _empresaService.ObtenerEmpresaActualAsync();

                ViewBag.Categorias = categorias.Select(c => new CategoriaViewModel
                {
                    Id = c.Id,
                    Nombre = c.Nombre,
                    Descripcion = c.Descripcion
                }).ToList();

                ViewBag.Insumos = insumos.Where(i => i.Activo).Select(i => new InsumoViewModel
                {
                    Id = i.Id,
                    Nombre = i.Nombre,
                    UnidadMedida = i.UnidadMedida,
                    PrecioUnitario = i.PrecioUnitario,
                    Cantidad = i.Cantidad
                }).ToList();

                // Pasar el margen de ganancia para el cálculo del precio sugerido en JavaScript
                ViewBag.MargenGanancia = empresa?.MargenGanancia ?? 0;

                return View(new ProductoVentaViewModel());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar vista de crear producto");
                TempData["ErrorMessage"] = "Ocurrió un error al cargar el formulario.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Procesa la creación de un nuevo producto
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductoVentaViewModel model, string? sucursalId, List<string>? insumoIds, List<decimal>? cantidades)
        {
            if (!ModelState.IsValid)
            {
                var categorias = await _inventarioService.ObtenerCategoriasAsync();
                var insumos = await _inventarioService.ObtenerInsumosAsync();
                var empresa = await _empresaService.ObtenerEmpresaActualAsync();

                ViewBag.Categorias = categorias.Select(c => new CategoriaViewModel
                {
                    Id = c.Id,
                    Nombre = c.Nombre,
                    Descripcion = c.Descripcion
                }).ToList();

                ViewBag.Insumos = insumos.Where(i => i.Activo).Select(i => new InsumoViewModel
                {
                    Id = i.Id,
                    Nombre = i.Nombre,
                    UnidadMedida = i.UnidadMedida,
                    PrecioUnitario = i.PrecioUnitario,
                    Cantidad = i.Cantidad
                }).ToList();

                ViewBag.MargenGanancia = empresa?.MargenGanancia ?? 0;

                return View(model);
            }

            try
            {
                var dto = new ProductoVentaDto
                {
                    CategoriaId = string.IsNullOrEmpty(model.CategoriaId) ? null : model.CategoriaId,
                    Nombre = model.Nombre,
                    Descripcion = model.Descripcion,
                    PrecioVenta = model.PrecioVenta,
                    Activo = model.Activo,
                    SucursalId = sucursalId ?? string.Empty
                };

                var producto = await _productoService.CrearProductoAsync(dto);
                if (producto == null)
                {
                    TempData["ErrorMessage"] = "No se pudo crear el producto.";
                    return RedirectToAction(nameof(Create));
                }

                // Asociar insumos si se proporcionaron
                if (insumoIds != null && cantidades != null && insumoIds.Count == cantidades.Count)
                {
                    for (int i = 0; i < insumoIds.Count; i++)
                    {
                        if (!string.IsNullOrEmpty(insumoIds[i]) && cantidades[i] > 0)
                        {
                            await _productoService.AsociarInsumoAsync(producto.Id!, insumoIds[i], cantidades[i]);
                        }
                    }
                }

                TempData["SuccessMessage"] = "Producto creado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear producto");
                TempData["ErrorMessage"] = $"Error al crear el producto: {ex.Message}";
            }

            var categorias2 = await _inventarioService.ObtenerCategoriasAsync();
            var insumos2 = await _inventarioService.ObtenerInsumosAsync();
            var empresaError = await _empresaService.ObtenerEmpresaActualAsync();

            ViewBag.Categorias = categorias2.Select(c => new CategoriaViewModel
            {
                Id = c.Id,
                Nombre = c.Nombre,
                Descripcion = c.Descripcion
            }).ToList();

            ViewBag.Insumos = insumos2.Where(i => i.Activo).Select(i => new InsumoViewModel
            {
                Id = i.Id,
                Nombre = i.Nombre,
                UnidadMedida = i.UnidadMedida,
                PrecioUnitario = i.PrecioUnitario,
                Cantidad = i.Cantidad
            }).ToList();

            ViewBag.MargenGanancia = empresaError?.MargenGanancia ?? 0;

            return View(model);
        }

        /// <summary>
        /// Vista para editar un producto existente
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "Id de producto inválido.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var producto = await _productoService.ObtenerProductoPorIdAsync(id);
                if (producto == null)
                {
                    TempData["ErrorMessage"] = "Producto no encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                var insumosProducto = await _productoService.ObtenerInsumosDelProductoAsync(id);
                var todosInsumos = await _inventarioService.ObtenerInsumosAsync();
                var insumosDict = todosInsumos.ToDictionary(i => i.Id!, i => i);

                var insumosViewModel = new List<InsumoProductoViewModel>();
                foreach (var ip in insumosProducto)
                {
                    if (insumosDict.ContainsKey(ip.InsumoId))
                    {
                        var insumo = insumosDict[ip.InsumoId];
                        insumosViewModel.Add(new InsumoProductoViewModel
                        {
                            Id = ip.Id,
                            InsumoId = ip.InsumoId,
                            InsumoNombre = insumo.Nombre,
                            CantidadUtilizada = ip.CantidadUtilizada,
                            PrecioUnitarioInsumo = insumo.PrecioUnitario,
                            CostoTotal = ip.CantidadUtilizada * insumo.PrecioUnitario,
                            StockDisponible = insumo.Cantidad,
                            StockSuficiente = insumo.Cantidad >= ip.CantidadUtilizada
                        });
                    }
                }

                var precioSugerido = await _productoService.CalcularPrecioSugeridoAsync(id);
                var categorias = await _inventarioService.ObtenerCategoriasAsync();
                var insumos = await _inventarioService.ObtenerInsumosAsync();
                var empresa = await _empresaService.ObtenerEmpresaActualAsync();

                ViewBag.Categorias = categorias.Select(c => new CategoriaViewModel
                {
                    Id = c.Id,
                    Nombre = c.Nombre,
                    Descripcion = c.Descripcion
                }).ToList();

                ViewBag.Insumos = insumos.Where(i => i.Activo).Select(i => new InsumoViewModel
                {
                    Id = i.Id,
                    Nombre = i.Nombre,
                    UnidadMedida = i.UnidadMedida,
                    PrecioUnitario = i.PrecioUnitario,
                    Cantidad = i.Cantidad
                }).ToList();

                ViewBag.MargenGanancia = empresa?.MargenGanancia ?? 0;

                var viewModel = new ProductoVentaViewModel
                {
                    Id = producto.Id,
                    CategoriaId = producto.CategoriaId,
                    Nombre = producto.Nombre,
                    Descripcion = producto.Descripcion,
                    PrecioVenta = producto.PrecioVenta,
                    PrecioSugerido = precioSugerido,
                    Activo = producto.Activo,
                    Insumos = insumosViewModel
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar vista de editar producto {ProductoId}", id);
                TempData["ErrorMessage"] = "Ocurrió un error al cargar el producto.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Procesa la actualización de un producto
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductoVentaViewModel model, List<string>? insumoIds, List<decimal>? cantidades)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Edit), new { id = model.Id });
            }

            if (string.IsNullOrEmpty(model.Id))
            {
                TempData["ErrorMessage"] = "Id de producto inválido.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var productoExistente = await _productoService.ObtenerProductoPorIdAsync(model.Id);
                if (productoExistente == null)
                {
                    TempData["ErrorMessage"] = "Producto no encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                var dto = new ProductoVentaDto
                {
                    Id = model.Id,
                    EmpresaId = productoExistente.EmpresaId,
                    SucursalId = productoExistente.SucursalId,
                    CategoriaId = string.IsNullOrEmpty(model.CategoriaId) ? null : model.CategoriaId,
                    Nombre = model.Nombre,
                    Descripcion = model.Descripcion,
                    PrecioVenta = model.PrecioVenta,
                    Activo = model.Activo
                };

                var resultado = await _productoService.ActualizarProductoAsync(dto);
                if (!resultado)
                {
                    TempData["ErrorMessage"] = "No se pudo actualizar el producto.";
                    return RedirectToAction(nameof(Edit), new { id = model.Id });
                }

                // Actualizar insumos si se proporcionaron
                if (insumoIds != null && cantidades != null)
                {
                    // Obtener insumos actuales
                    var insumosActuales = await _productoService.ObtenerInsumosDelProductoAsync(model.Id);
                    var insumosActualesIds = insumosActuales.Select(i => i.InsumoId).ToList();

                    // Eliminar insumos que ya no están
                    foreach (var insumoActual in insumosActuales)
                    {
                        if (!insumoIds.Contains(insumoActual.InsumoId))
                        {
                            await _productoService.EliminarInsumoProductoAsync(insumoActual.Id!);
                        }
                    }

                    // Agregar o actualizar insumos
                    for (int i = 0; i < insumoIds.Count; i++)
                    {
                        if (!string.IsNullOrEmpty(insumoIds[i]) && cantidades[i] > 0)
                        {
                            var insumoExistente = insumosActuales.FirstOrDefault(ia => ia.InsumoId == insumoIds[i]);
                            if (insumoExistente != null)
                            {
                                // Actualizar cantidad
                                insumoExistente.CantidadUtilizada = cantidades[i];
                                await _productoService.ActualizarInsumoProductoAsync(insumoExistente.Id!, insumoExistente);
                            }
                            else
                            {
                                // Agregar nuevo insumo
                                await _productoService.AsociarInsumoAsync(model.Id, insumoIds[i], cantidades[i]);
                            }
                        }
                    }
                }

                TempData["SuccessMessage"] = "Producto actualizado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar producto {ProductoId}", model.Id);
                TempData["ErrorMessage"] = $"Error al actualizar el producto: {ex.Message}";
            }

            return RedirectToAction(nameof(Edit), new { id = model.Id });
        }

        /// <summary>
        /// API endpoint para calcular precio sugerido
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> CalcularPrecioSugerido(string productoId)
        {
            try
            {
                var precioSugerido = await _productoService.CalcularPrecioSugeridoAsync(productoId);
                return Json(new { precioSugerido });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al calcular precio sugerido");
                return Json(new { error = ex.Message });
            }
        }

        /// <summary>
        /// API endpoint para validar stock disponible
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ValidarStock(string productoId, decimal cantidad = 1)
        {
            try
            {
                var (stockSuficiente, insumosInsuficientes) = await _productoService.ValidarStockDisponibleAsync(productoId, cantidad);
                return Json(new { stockSuficiente, insumosInsuficientes });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar stock");
                return Json(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Elimina un producto (eliminación lógica)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "Id de producto inválido.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var resultado = await _productoService.EliminarProductoAsync(id);
                TempData[resultado ? "SuccessMessage" : "ErrorMessage"] =
                    resultado ? "Producto eliminado correctamente." : "No se pudo eliminar el producto.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar producto {ProductoId}", id);
                TempData["ErrorMessage"] = "Ocurrió un error al eliminar el producto.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

