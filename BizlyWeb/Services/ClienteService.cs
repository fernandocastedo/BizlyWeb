using BizlyWeb.Models.DTOs;
using Microsoft.AspNetCore.Http;

namespace BizlyWeb.Services
{
    /// <summary>
    /// Servicio de negocio para administración de clientes.
    /// Capa de Negocio
    /// </summary>
    public class ClienteService
    {
        private readonly ApiService _apiService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly VentaService _ventaService;

        public ClienteService(
            ApiService apiService,
            IHttpContextAccessor httpContextAccessor,
            VentaService ventaService)
        {
            _apiService = apiService;
            _httpContextAccessor = httpContextAccessor;
            _ventaService = ventaService;
        }

        private string? GetEmpresaId()
        {
            return SessionHelper.GetEmpresaId(_httpContextAccessor);
        }

        /// <summary>
        /// Obtiene todos los clientes de la empresa actual
        /// </summary>
        public async Task<List<ClienteDto>> ObtenerClientesAsync()
        {
            var empresaId = GetEmpresaId();
            if (string.IsNullOrEmpty(empresaId))
            {
                return new List<ClienteDto>();
            }

            var data = await _apiService.GetAsync<List<ClienteDto>>("/api/clientes") ?? new List<ClienteDto>();
            return data.Where(c => c.EmpresaId == empresaId).ToList();
        }

        /// <summary>
        /// Obtiene un cliente por ID
        /// </summary>
        public async Task<ClienteDto?> ObtenerClientePorIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            return await _apiService.GetAsync<ClienteDto>($"/api/clientes/{id}");
        }

        /// <summary>
        /// Crea un nuevo cliente
        /// </summary>
        public async Task<ClienteDto?> CrearClienteAsync(ClienteDto cliente)
        {
            var empresaId = GetEmpresaId();
            if (string.IsNullOrEmpty(empresaId))
            {
                throw new InvalidOperationException("No se pudo obtener la empresa actual.");
            }

            cliente.EmpresaId = empresaId;
            cliente.CreatedAt = DateTime.UtcNow;

            return await _apiService.PostAsync<ClienteDto, ClienteDto>("/api/clientes", cliente);
        }

        /// <summary>
        /// Actualiza un cliente existente
        /// </summary>
        public async Task<bool> ActualizarClienteAsync(string id, ClienteDto cliente)
        {
            if (string.IsNullOrEmpty(id) || cliente == null)
            {
                return false;
            }

            var empresaId = GetEmpresaId();
            if (string.IsNullOrEmpty(empresaId))
            {
                throw new InvalidOperationException("No se pudo obtener la empresa actual.");
            }

            cliente.EmpresaId = empresaId;
            cliente.Id = id;

            var response = await _apiService.PutAsync<ClienteDto, ClienteDto>($"/api/clientes/{id}", cliente);
            return response != null || true; // PUT retorna 204, consideramos éxito si no hubo excepción
        }

        /// <summary>
        /// Elimina un cliente
        /// </summary>
        public async Task<bool> EliminarClienteAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return false;
            }

            try
            {
                await _apiService.DeleteAsync($"/api/clientes/{id}");
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Obtiene clientes filtrados por nombre, NIT, email o teléfono (RF-26)
        /// </summary>
        public async Task<List<ClienteDto>> ObtenerClientesFiltradosAsync(
            string? nombre = null,
            int? nit = null,
            string? email = null,
            string? telefono = null)
        {
            var clientes = await ObtenerClientesAsync();

            if (!string.IsNullOrEmpty(nombre))
            {
                clientes = clientes.Where(c => c.Nombre.Contains(nombre, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (nit.HasValue)
            {
                clientes = clientes.Where(c => c.Nit == nit.Value).ToList();
            }

            if (!string.IsNullOrEmpty(email))
            {
                clientes = clientes.Where(c => c.Email.Contains(email, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrEmpty(telefono))
            {
                clientes = clientes.Where(c => c.Telefono.Contains(telefono, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return clientes.OrderBy(c => c.Nombre).ToList();
        }

        /// <summary>
        /// Obtiene el top de clientes por total de compras (RF-40)
        /// </summary>
        public async Task<List<(string ClienteId, string ClienteNombre, int TotalCompras, decimal TotalGastado)>> ObtenerTopClientesAsync(DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            var ventas = await _ventaService.ObtenerVentasFiltradasAsync(fechaInicio, fechaFin, estadoPedido: "completado");
            
            // Filtrar solo ventas con cliente asignado
            var ventasConCliente = ventas
                .Where(v => !string.IsNullOrEmpty(v.ClienteId) && v.EstadoPedido == "completado")
                .ToList();

            var topClientes = ventasConCliente
                .GroupBy(v => v.ClienteId!)
                .Select(g => new
                {
                    ClienteId = g.Key,
                    TotalCompras = g.Count(),
                    TotalGastado = g.Sum(v => v.Total)
                })
                .OrderByDescending(x => x.TotalGastado)
                .Take(10)
                .ToList();

            // Obtener nombres de clientes
            var clientesDict = new Dictionary<string, string>();
            foreach (var topCliente in topClientes)
            {
                var cliente = await ObtenerClientePorIdAsync(topCliente.ClienteId);
                if (cliente != null)
                {
                    clientesDict[topCliente.ClienteId] = cliente.Nombre;
                }
            }

            return topClientes.Select(tc => (
                tc.ClienteId,
                ClienteNombre: clientesDict.ContainsKey(tc.ClienteId) ? clientesDict[tc.ClienteId] : $"Cliente {tc.ClienteId.Substring(0, Math.Min(8, tc.ClienteId.Length))}",
                tc.TotalCompras,
                tc.TotalGastado
            )).ToList();
        }
    }
}


