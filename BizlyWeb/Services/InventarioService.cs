using BizlyWeb.Models.DTOs;
using Microsoft.AspNetCore.Http;

namespace BizlyWeb.Services
{
    /// <summary>
    /// Servicio de negocio para administración de inventario (insumos y registros).
    /// Capa de Negocio
    /// </summary>
    public class InventarioService
    {
        private readonly ApiService _apiService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public InventarioService(ApiService apiService, IHttpContextAccessor httpContextAccessor)
        {
            _apiService = apiService;
            _httpContextAccessor = httpContextAccessor;
        }

        private string? GetEmpresaId()
        {
            return _httpContextAccessor.HttpContext?.Session.GetInt32("EmpresaId")?.ToString()
                   ?? _httpContextAccessor.HttpContext?.Session.GetString("EmpresaId");
        }

        private string? GetUsuarioId()
        {
            return _httpContextAccessor.HttpContext?.Session.GetInt32("UsuarioId")?.ToString()
                   ?? _httpContextAccessor.HttpContext?.Session.GetString("UsuarioId");
        }

        /// <summary>
        /// Obtiene todos los insumos de la empresa actual
        /// </summary>
        public async Task<List<InsumoDto>> ObtenerInsumosAsync()
        {
            var empresaId = GetEmpresaId();
            if (string.IsNullOrEmpty(empresaId))
            {
                return new List<InsumoDto>();
            }

            var data = await _apiService.GetAsync<List<InsumoDto>>("/api/insumos") ?? new List<InsumoDto>();
            return data.Where(i => i.EmpresaId == empresaId).ToList();
        }

        /// <summary>
        /// Obtiene un insumo por ID
        /// </summary>
        public async Task<InsumoDto?> ObtenerInsumoPorIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            return await _apiService.GetAsync<InsumoDto>($"/api/insumos/{id}");
        }

        /// <summary>
        /// Crea un nuevo insumo
        /// </summary>
        public async Task<InsumoDto?> CrearInsumoAsync(InsumoDto insumo)
        {
            var empresaId = GetEmpresaId();
            if (string.IsNullOrEmpty(empresaId))
            {
                throw new InvalidOperationException("No se pudo obtener la empresa actual.");
            }

            // Asegurar que el insumo pertenezca a la empresa actual
            insumo.EmpresaId = empresaId;

            // Si no se especifica sucursal, obtener la primera disponible
            if (string.IsNullOrEmpty(insumo.SucursalId))
            {
                var sucursalService = new SucursalService(_apiService, _httpContextAccessor);
                var sucursales = await sucursalService.ObtenerSucursalesAsync();
                if (sucursales.Any())
                {
                    insumo.SucursalId = sucursales.First().Id!;
                }
                else
                {
                    throw new InvalidOperationException("No hay sucursales disponibles. Crea una sucursal primero.");
                }
            }

            // Calcular PrecioTotal
            insumo.PrecioTotal = insumo.Cantidad * insumo.PrecioUnitario;

            return await _apiService.PostAsync<InsumoDto, InsumoDto>("/api/insumos", insumo);
        }

        /// <summary>
        /// Actualiza un insumo existente
        /// </summary>
        public async Task<bool> ActualizarInsumoAsync(InsumoDto insumo)
        {
            if (insumo == null || string.IsNullOrEmpty(insumo.Id))
            {
                return false;
            }

            // Recalcular PrecioTotal
            insumo.PrecioTotal = insumo.Cantidad * insumo.PrecioUnitario;

            var response = await _apiService.PutAsync<InsumoDto, InsumoDto>($"/api/insumos/{insumo.Id}", insumo);
            return response != null;
        }

        /// <summary>
        /// Elimina un insumo (eliminación lógica: Activo = false)
        /// </summary>
        public async Task<bool> EliminarInsumoAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return false;
            }

            var insumo = await ObtenerInsumoPorIdAsync(id);
            if (insumo == null)
            {
                return false;
            }

            insumo.Activo = false;
            return await ActualizarInsumoAsync(insumo);
        }

        /// <summary>
        /// Actualiza el stock de un insumo y crea un registro de inventario
        /// </summary>
        public async Task<bool> ActualizarStockAsync(string insumoId, decimal nuevaCantidad, string tipoMovimiento, string motivo)
        {
            var insumo = await ObtenerInsumoPorIdAsync(insumoId);
            if (insumo == null)
            {
                return false;
            }

            var empresaId = GetEmpresaId();
            var usuarioId = GetUsuarioId();
            if (string.IsNullOrEmpty(empresaId) || string.IsNullOrEmpty(usuarioId))
            {
                throw new InvalidOperationException("No se pudo obtener la empresa o usuario actual.");
            }

            var cantidadAnterior = insumo.Cantidad;
            insumo.Cantidad = nuevaCantidad;
            insumo.PrecioTotal = insumo.Cantidad * insumo.PrecioUnitario;

            // Actualizar el insumo
            var actualizado = await ActualizarInsumoAsync(insumo);
            if (!actualizado)
            {
                return false;
            }

            // Crear registro de inventario
            var registro = new RegistroInventarioDto
            {
                EmpresaId = empresaId,
                SucursalId = insumo.SucursalId,
                UsuarioId = usuarioId,
                InsumoId = insumoId,
                TipoMovimiento = tipoMovimiento, // "entrada", "salida", "ajuste"
                CantidadAnterior = cantidadAnterior,
                CantidadNueva = nuevaCantidad,
                Motivo = motivo
            };

            await _apiService.PostAsync<RegistroInventarioDto, RegistroInventarioDto>("/api/registrosinventario", registro);

            return true;
        }

        /// <summary>
        /// Obtiene todos los registros de inventario
        /// </summary>
        public async Task<List<RegistroInventarioDto>> ObtenerRegistrosInventarioAsync()
        {
            var empresaId = GetEmpresaId();
            if (string.IsNullOrEmpty(empresaId))
            {
                return new List<RegistroInventarioDto>();
            }

            var data = await _apiService.GetAsync<List<RegistroInventarioDto>>("/api/registrosinventario") ?? new List<RegistroInventarioDto>();
            return data.Where(r => r.EmpresaId == empresaId).ToList();
        }

        /// <summary>
        /// Obtiene los registros de inventario de un insumo específico
        /// </summary>
        public async Task<List<RegistroInventarioDto>> ObtenerRegistrosPorInsumoAsync(string insumoId)
        {
            if (string.IsNullOrEmpty(insumoId))
            {
                return new List<RegistroInventarioDto>();
            }

            var data = await _apiService.GetAsync<List<RegistroInventarioDto>>($"/api/registrosinventario/por-insumo/{insumoId}") ?? new List<RegistroInventarioDto>();
            return data;
        }

        /// <summary>
        /// Obtiene los insumos con stock bajo (Cantidad <= StockMinimo)
        /// </summary>
        public async Task<List<InsumoDto>> ObtenerInsumosStockBajoAsync()
        {
            var insumos = await ObtenerInsumosAsync();
            return insumos.Where(i => i.Activo && i.Cantidad <= i.StockMinimo).ToList();
        }

        /// <summary>
        /// Obtiene todas las categorías de la empresa actual
        /// </summary>
        public async Task<List<CategoriaDto>> ObtenerCategoriasAsync()
        {
            var empresaId = GetEmpresaId();
            if (string.IsNullOrEmpty(empresaId))
            {
                return new List<CategoriaDto>();
            }

            var data = await _apiService.GetAsync<List<CategoriaDto>>("/api/categorias") ?? new List<CategoriaDto>();
            return data.Where(c => c.EmpresaId == empresaId).ToList();
        }
    }
}

