using BizlyWeb.Attributes;
using BizlyWeb.Models.DTOs;
using BizlyWeb.Models.ViewModels;
using BizlyWeb.Services;
using Microsoft.AspNetCore.Mvc;

namespace BizlyWeb.Controllers
{
    [AuthorizeRole("EMPRENDEDOR")]
    public class CostosGastosController : Controller
    {
        private readonly CostoGastoService _costoGastoService;
        private readonly SucursalService _sucursalService;
        private readonly InventarioService _inventarioService;
        private readonly ILogger<CostosGastosController> _logger;

        public CostosGastosController(
            CostoGastoService costoGastoService,
            SucursalService sucursalService,
            InventarioService inventarioService,
            ILogger<CostosGastosController> logger)
        {
            _costoGastoService = costoGastoService;
            _sucursalService = sucursalService;
            _inventarioService = inventarioService;
            _logger = logger;
        }

        /// <summary>
        /// Vista principal: Lista de costos y gastos con filtros y alertas (RF-32, RF-34, RF-36, RF-37)
        /// </summary>
        public async Task<IActionResult> Index(
            DateTime? fechaInicio,
            DateTime? fechaFin,
            string? categoriaFinanciera,
            string? clasificacion,
            string? sucursalId)
        {
            try
            {
                var costosGastos = await _costoGastoService.ObtenerCostosGastosFiltradosAsync(
                    fechaInicio, fechaFin, categoriaFinanciera, clasificacion, sucursalId);

                var alertasIncremento = await _costoGastoService.DetectarIncrementosCostosAsync();

                var viewModel = new CostoGastoIndexViewModel
                {
                    CostosGastos = costosGastos.Select(cg => new CostoGastoViewModel
                    {
                        Id = cg.Id,
                        Descripcion = cg.Descripcion,
                        Monto = cg.Monto,
                        Fecha = cg.Fecha,
                        CategoriaFinanciera = cg.CategoriaFinanciera,
                        Clasificacion = cg.Clasificacion,
                        SucursalId = cg.SucursalId,
                        InsumoId = cg.InsumoId,
                        TrabajadorId = cg.TrabajadorId
                    }).ToList(),
                    AlertasIncremento = alertasIncremento.Select(a => new AlertaIncrementoCostoViewModel
                    {
                        Categoria = a.Categoria,
                        MontoAnterior = a.MontoAnterior,
                        MontoActual = a.MontoActual,
                        PorcentajeIncremento = a.PorcentajeIncremento
                    }).ToList(),
                    FiltroFechaInicio = fechaInicio,
                    FiltroFechaFin = fechaFin,
                    FiltroCategoriaFinanciera = categoriaFinanciera,
                    FiltroClasificacion = clasificacion,
                    FiltroSucursalId = sucursalId,
                    TotalCostosGastos = costosGastos.Sum(cg => cg.Monto),
                    TotalDirectos = costosGastos.Where(cg => cg.CategoriaFinanciera == "DIRECTO").Sum(cg => cg.Monto),
                    TotalAdministrativos = costosGastos.Where(cg => cg.CategoriaFinanciera == "ADMINISTRATIVO").Sum(cg => cg.Monto),
                    TotalFijos = costosGastos.Where(cg => cg.Clasificacion == "FIJO").Sum(cg => cg.Monto),
                    TotalVariables = costosGastos.Where(cg => cg.Clasificacion == "VARIABLE").Sum(cg => cg.Monto)
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener costos y gastos");
                TempData["Error"] = "Error al cargar la lista de costos y gastos. Por favor, intente nuevamente.";
                return View(new CostoGastoIndexViewModel());
            }
        }

        /// <summary>
        /// Vista para crear un nuevo costo/gasto (RF-32)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            try
            {
                var sucursales = await _sucursalService.ObtenerSucursalesAsync();
                var insumos = await _inventarioService.ObtenerInsumosAsync();

                var viewModel = new CostoGastoViewModel
                {
                    Fecha = DateTime.Today,
                    Sucursales = sucursales.Select(s => new SucursalViewModel
                    {
                        Id = s.Id,
                        Nombre = s.Nombre,
                        Direccion = s.Direccion,
                        Ciudad = s.Ciudad,
                        Departamento = s.Departamento
                    }).ToList(),
                    Insumos = insumos.Select(i => new InsumoViewModel
                    {
                        Id = i.Id,
                        Nombre = i.Nombre,
                        Descripcion = i.Descripcion
                    }).ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar formulario de creación de costo/gasto");
                TempData["Error"] = "Error al cargar el formulario. Por favor, intente nuevamente.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Procesa la creación de un nuevo costo/gasto (RF-32, RF-34)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CostoGastoViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                var sucursales = await _sucursalService.ObtenerSucursalesAsync();
                var insumos = await _inventarioService.ObtenerInsumosAsync();
                viewModel.Sucursales = sucursales.Select(s => new SucursalViewModel
                {
                    Id = s.Id,
                    Nombre = s.Nombre,
                    Direccion = s.Direccion,
                    Ciudad = s.Ciudad,
                    Departamento = s.Departamento
                }).ToList();
                viewModel.Insumos = insumos.Select(i => new InsumoViewModel
                {
                    Id = i.Id,
                    Nombre = i.Nombre,
                    Descripcion = i.Descripcion
                }).ToList();
                return View(viewModel);
            }

            try
            {
                var costoGastoDto = new CostoGastoDto
                {
                    Descripcion = viewModel.Descripcion,
                    Monto = viewModel.Monto,
                    Fecha = viewModel.Fecha,
                    CategoriaFinanciera = viewModel.CategoriaFinanciera,
                    Clasificacion = viewModel.Clasificacion,
                    SucursalId = string.IsNullOrEmpty(viewModel.SucursalId) ? null : viewModel.SucursalId,
                    InsumoId = string.IsNullOrEmpty(viewModel.InsumoId) ? null : viewModel.InsumoId,
                    TrabajadorId = string.IsNullOrEmpty(viewModel.TrabajadorId) ? null : viewModel.TrabajadorId
                };

                var resultado = await _costoGastoService.CrearCostoGastoAsync(costoGastoDto);
                if (resultado != null)
                {
                    TempData["Success"] = "Costo/Gasto registrado exitosamente.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["Error"] = "No se pudo registrar el costo/gasto. Por favor, intente nuevamente.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear costo/gasto");
                TempData["Error"] = $"Error al registrar el costo/gasto: {ex.Message}";
            }

            var sucursalesRetry = await _sucursalService.ObtenerSucursalesAsync();
            var insumosRetry = await _inventarioService.ObtenerInsumosAsync();
            viewModel.Sucursales = sucursalesRetry.Select(s => new SucursalViewModel
            {
                Id = s.Id,
                Nombre = s.Nombre,
                Direccion = s.Direccion,
                Ciudad = s.Ciudad,
                Departamento = s.Departamento
            }).ToList();
            viewModel.Insumos = insumosRetry.Select(i => new InsumoViewModel
            {
                Id = i.Id,
                Nombre = i.Nombre,
                Descripcion = i.Descripcion
            }).ToList();
            return View(viewModel);
        }

        /// <summary>
        /// Vista para editar un costo/gasto existente (RF-35)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "ID de costo/gasto no válido.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var costoGasto = await _costoGastoService.ObtenerCostoGastoPorIdAsync(id);
                if (costoGasto == null)
                {
                    TempData["Error"] = "Costo/Gasto no encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                var sucursales = await _sucursalService.ObtenerSucursalesAsync();
                var insumos = await _inventarioService.ObtenerInsumosAsync();

                var viewModel = new CostoGastoViewModel
                {
                    Id = costoGasto.Id,
                    Descripcion = costoGasto.Descripcion,
                    Monto = costoGasto.Monto,
                    Fecha = costoGasto.Fecha,
                    CategoriaFinanciera = costoGasto.CategoriaFinanciera,
                    Clasificacion = costoGasto.Clasificacion,
                    SucursalId = costoGasto.SucursalId,
                    InsumoId = costoGasto.InsumoId,
                    TrabajadorId = costoGasto.TrabajadorId,
                    Sucursales = sucursales.Select(s => new SucursalViewModel
                    {
                        Id = s.Id,
                        Nombre = s.Nombre,
                        Direccion = s.Direccion,
                        Ciudad = s.Ciudad,
                        Departamento = s.Departamento
                    }).ToList(),
                    Insumos = insumos.Select(i => new InsumoViewModel
                    {
                        Id = i.Id,
                        Nombre = i.Nombre,
                        Descripcion = i.Descripcion
                    }).ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar costo/gasto para edición");
                TempData["Error"] = "Error al cargar el costo/gasto. Por favor, intente nuevamente.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Procesa la actualización de un costo/gasto (RF-35)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, CostoGastoViewModel viewModel)
        {
            if (string.IsNullOrEmpty(id) || id != viewModel.Id)
            {
                TempData["Error"] = "ID de costo/gasto no válido.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                var sucursales = await _sucursalService.ObtenerSucursalesAsync();
                var insumos = await _inventarioService.ObtenerInsumosAsync();
                viewModel.Sucursales = sucursales.Select(s => new SucursalViewModel
                {
                    Id = s.Id,
                    Nombre = s.Nombre,
                    Direccion = s.Direccion,
                    Ciudad = s.Ciudad,
                    Departamento = s.Departamento
                }).ToList();
                viewModel.Insumos = insumos.Select(i => new InsumoViewModel
                {
                    Id = i.Id,
                    Nombre = i.Nombre,
                    Descripcion = i.Descripcion
                }).ToList();
                return View(viewModel);
            }

            try
            {
                var costoGastoDto = new CostoGastoDto
                {
                    Id = viewModel.Id,
                    Descripcion = viewModel.Descripcion,
                    Monto = viewModel.Monto,
                    Fecha = viewModel.Fecha,
                    CategoriaFinanciera = viewModel.CategoriaFinanciera,
                    Clasificacion = viewModel.Clasificacion,
                    SucursalId = string.IsNullOrEmpty(viewModel.SucursalId) ? null : viewModel.SucursalId,
                    InsumoId = string.IsNullOrEmpty(viewModel.InsumoId) ? null : viewModel.InsumoId,
                    TrabajadorId = string.IsNullOrEmpty(viewModel.TrabajadorId) ? null : viewModel.TrabajadorId
                };

                var resultado = await _costoGastoService.ActualizarCostoGastoAsync(id, costoGastoDto);
                if (resultado)
                {
                    TempData["Success"] = "Costo/Gasto actualizado exitosamente.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["Error"] = "No se pudo actualizar el costo/gasto. Por favor, intente nuevamente.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar costo/gasto");
                TempData["Error"] = $"Error al actualizar el costo/gasto: {ex.Message}";
            }

            var sucursalesRetry = await _sucursalService.ObtenerSucursalesAsync();
            var insumosRetry = await _inventarioService.ObtenerInsumosAsync();
            viewModel.Sucursales = sucursalesRetry.Select(s => new SucursalViewModel
            {
                Id = s.Id,
                Nombre = s.Nombre,
                Direccion = s.Direccion,
                Ciudad = s.Ciudad,
                Departamento = s.Departamento
            }).ToList();
            viewModel.Insumos = insumosRetry.Select(i => new InsumoViewModel
            {
                Id = i.Id,
                Nombre = i.Nombre,
                Descripcion = i.Descripcion
            }).ToList();
            return View(viewModel);
        }

        /// <summary>
        /// Elimina un costo/gasto (RF-35)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "ID de costo/gasto no válido.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var resultado = await _costoGastoService.EliminarCostoGastoAsync(id);
                if (resultado)
                {
                    TempData["Success"] = "Costo/Gasto eliminado exitosamente.";
                }
                else
                {
                    TempData["Error"] = "No se pudo eliminar el costo/gasto. Por favor, intente nuevamente.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar costo/gasto");
                TempData["Error"] = $"Error al eliminar el costo/gasto: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}


