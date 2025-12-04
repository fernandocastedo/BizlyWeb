using BizlyWeb.Models.DTOs;
using Microsoft.AspNetCore.Http;

namespace BizlyWeb.Services
{
    /// <summary>
    /// Servicio de negocio para administración de categorías.
    /// Capa de Negocio
    /// </summary>
    public class CategoriaService
    {
        private readonly ApiService _apiService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CategoriaService(
            ApiService apiService,
            IHttpContextAccessor httpContextAccessor)
        {
            _apiService = apiService;
            _httpContextAccessor = httpContextAccessor;
        }

        private string? GetEmpresaId()
        {
            return SessionHelper.GetEmpresaId(_httpContextAccessor);
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

        /// <summary>
        /// Obtiene una categoría por ID
        /// </summary>
        public async Task<CategoriaDto?> ObtenerCategoriaPorIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            return await _apiService.GetAsync<CategoriaDto>($"/api/categorias/{id}");
        }

        /// <summary>
        /// Crea una nueva categoría
        /// </summary>
        public async Task<CategoriaDto?> CrearCategoriaAsync(CategoriaDto categoria)
        {
            var empresaId = GetEmpresaId();
            if (string.IsNullOrEmpty(empresaId))
            {
                throw new InvalidOperationException("No se pudo obtener la empresa actual.");
            }

            categoria.EmpresaId = empresaId;
            categoria.CreatedAt = DateTime.UtcNow;

            return await _apiService.PostAsync<CategoriaDto, CategoriaDto>("/api/categorias", categoria);
        }

        /// <summary>
        /// Actualiza una categoría existente
        /// </summary>
        public async Task<bool> ActualizarCategoriaAsync(string id, CategoriaDto categoria)
        {
            if (string.IsNullOrEmpty(id) || categoria == null)
            {
                return false;
            }

            var empresaId = GetEmpresaId();
            if (string.IsNullOrEmpty(empresaId))
            {
                throw new InvalidOperationException("No se pudo obtener la empresa actual.");
            }

            categoria.EmpresaId = empresaId;
            categoria.Id = id;

            try
            {
                await _apiService.PutAsync<CategoriaDto, CategoriaDto>($"/api/categorias/{id}", categoria);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Elimina una categoría
        /// </summary>
        public async Task<bool> EliminarCategoriaAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return false;
            }

            try
            {
                await _apiService.DeleteAsync($"/api/categorias/{id}");
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}


