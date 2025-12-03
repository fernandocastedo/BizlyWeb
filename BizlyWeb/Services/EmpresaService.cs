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

            try
            {
                // PUT retorna 204 NoContent, así que si no hay excepción, fue exitoso
                await _apiService.PutAsync<EmpresaDto, EmpresaDto>($"/api/empresas/{empresa.Id}", empresa);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}

