using BizlyWeb.Models.DTOs;

namespace BizlyWeb.Services
{
    /// <summary>
    /// Servicio de negocio para administración de empresas del emprendimiento.
    /// </summary>
    public class EmpresaService
    {
        private readonly ApiService _apiService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EmpresaService(ApiService apiService, IHttpContextAccessor httpContextAccessor)
        {
            _apiService = apiService;
            _httpContextAccessor = httpContextAccessor;
        }

        private string? GetEmpresaId()
        {
            return SessionHelper.GetEmpresaId(_httpContextAccessor);
        }

        public async Task<EmpresaDto?> ObtenerEmpresaActualAsync()
        {
            var empresaId = GetEmpresaId();
            if (string.IsNullOrEmpty(empresaId))
            {
                return null;
            }

            return await _apiService.GetAsync<EmpresaDto>($"/api/empresas/{empresaId}");
        }

        public async Task<bool> ActualizarEmpresaAsync(EmpresaDto empresa)
        {
            if (empresa == null || string.IsNullOrEmpty(empresa.Id))
            {
                return false;
            }

            var response = await _apiService.PutAsync<EmpresaDto, EmpresaDto>($"/api/empresas/{empresa.Id}", empresa);
            // PUT retorna 204 sin contenido, nuestro ApiService devuelve default, así que consideramos éxito si no hubo excepción
            return response != null || true;
        }
    }
}

