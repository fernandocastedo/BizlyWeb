using BizlyWeb.Attributes;
using BizlyWeb.Models.DTOs;
using BizlyWeb.Models.ViewModels;
using BizlyWeb.Services;
using Microsoft.AspNetCore.Mvc;

namespace BizlyWeb.Controllers
{
    [AuthorizeRole("EMPRENDEDOR", "TRABAJADOR")]
    public class ClientesController : Controller
    {
        private readonly ClienteService _clienteService;
        private readonly SucursalService _sucursalService;
        private readonly ILogger<ClientesController> _logger;

        public ClientesController(
            ClienteService clienteService,
            SucursalService sucursalService,
            ILogger<ClientesController> logger)
        {
            _clienteService = clienteService;
            _sucursalService = sucursalService;
            _logger = logger;
        }

        /// <summary>
        /// Vista principal: Lista de clientes con filtros y top clientes (RF-26, RF-40)
        /// </summary>
        public async Task<IActionResult> Index(
            string? nombre,
            int? nit,
            string? email,
            string? telefono,
            DateTime? fechaInicio,
            DateTime? fechaFin)
        {
            try
            {
                var clientes = await _clienteService.ObtenerClientesFiltradosAsync(nombre, nit, email, telefono);
                var topClientes = await _clienteService.ObtenerTopClientesAsync(fechaInicio, fechaFin);

                var viewModel = new ClienteIndexViewModel
                {
                    Clientes = clientes.Select(c => new ClienteViewModel
                    {
                        Id = c.Id,
                        Nombre = c.Nombre,
                        Nit = c.Nit,
                        Telefono = c.Telefono,
                        Email = c.Email,
                        Direccion = c.Direccion,
                        SucursalId = c.SucursalId,
                        CreatedAt = c.CreatedAt
                    }).ToList(),
                    TopClientes = topClientes.Select(tc => new TopClienteViewModel
                    {
                        ClienteId = tc.ClienteId,
                        ClienteNombre = tc.ClienteNombre,
                        TotalCompras = tc.TotalCompras,
                        TotalGastado = tc.TotalGastado
                    }).ToList(),
                    FiltroNombre = nombre,
                    FiltroNit = nit,
                    FiltroEmail = email,
                    FiltroTelefono = telefono,
                    FiltroFechaInicio = fechaInicio,
                    FiltroFechaFin = fechaFin
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener clientes");
                TempData["Error"] = "Error al cargar la lista de clientes. Por favor, intente nuevamente.";
                return View(new ClienteIndexViewModel());
            }
        }

        /// <summary>
        /// Vista para crear un nuevo cliente
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            try
            {
                var sucursales = await _sucursalService.ObtenerSucursalesAsync();
                var viewModel = new ClienteViewModel
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
                _logger.LogError(ex, "Error al cargar formulario de creación de cliente");
                TempData["Error"] = "Error al cargar el formulario. Por favor, intente nuevamente.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Procesa la creación de un nuevo cliente
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClienteViewModel viewModel)
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
                var clienteDto = new ClienteDto
                {
                    Nombre = viewModel.Nombre,
                    Nit = viewModel.Nit,
                    Telefono = viewModel.Telefono,
                    Email = viewModel.Email,
                    Direccion = viewModel.Direccion,
                    SucursalId = string.IsNullOrEmpty(viewModel.SucursalId) ? null : viewModel.SucursalId
                };

                var resultado = await _clienteService.CrearClienteAsync(clienteDto);
                if (resultado != null)
                {
                    TempData["Success"] = "Cliente creado exitosamente.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["Error"] = "No se pudo crear el cliente. Por favor, intente nuevamente.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear cliente");
                TempData["Error"] = $"Error al crear el cliente: {ex.Message}";
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
        /// Vista para editar un cliente existente
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "ID de cliente no válido.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var cliente = await _clienteService.ObtenerClientePorIdAsync(id);
                if (cliente == null)
                {
                    TempData["Error"] = "Cliente no encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                var sucursales = await _sucursalService.ObtenerSucursalesAsync();
                var viewModel = new ClienteViewModel
                {
                    Id = cliente.Id,
                    Nombre = cliente.Nombre,
                    Nit = cliente.Nit,
                    Telefono = cliente.Telefono,
                    Email = cliente.Email,
                    Direccion = cliente.Direccion,
                    SucursalId = cliente.SucursalId,
                    CreatedAt = cliente.CreatedAt,
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
                _logger.LogError(ex, "Error al cargar cliente para edición");
                TempData["Error"] = "Error al cargar el cliente. Por favor, intente nuevamente.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Procesa la actualización de un cliente
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, ClienteViewModel viewModel)
        {
            if (string.IsNullOrEmpty(id) || id != viewModel.Id)
            {
                TempData["Error"] = "ID de cliente no válido.";
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
                var clienteDto = new ClienteDto
                {
                    Id = viewModel.Id,
                    Nombre = viewModel.Nombre,
                    Nit = viewModel.Nit,
                    Telefono = viewModel.Telefono,
                    Email = viewModel.Email,
                    Direccion = viewModel.Direccion,
                    SucursalId = string.IsNullOrEmpty(viewModel.SucursalId) ? null : viewModel.SucursalId,
                    CreatedAt = viewModel.CreatedAt
                };

                var resultado = await _clienteService.ActualizarClienteAsync(id, clienteDto);
                if (resultado)
                {
                    TempData["Success"] = "Cliente actualizado exitosamente.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["Error"] = "No se pudo actualizar el cliente. Por favor, intente nuevamente.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar cliente");
                TempData["Error"] = $"Error al actualizar el cliente: {ex.Message}";
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
        /// Elimina un cliente
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "ID de cliente no válido.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var resultado = await _clienteService.EliminarClienteAsync(id);
                if (resultado)
                {
                    TempData["Success"] = "Cliente eliminado exitosamente.";
                }
                else
                {
                    TempData["Error"] = "No se pudo eliminar el cliente. Por favor, intente nuevamente.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar cliente");
                TempData["Error"] = $"Error al eliminar el cliente: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

