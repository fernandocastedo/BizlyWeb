using BizlyWeb.Models.DTOs;

namespace BizlyWeb.Services
{
    public class SucursalService
    {
        private readonly ApiService _apiService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SucursalService(ApiService apiService, IHttpContextAccessor httpContextAccessor)
        {
            _apiService = apiService;
            _httpContextAccessor = httpContextAccessor;
        }

        private string? GetEmpresaId()
        {
            return SessionHelper.GetEmpresaId(_httpContextAccessor);
        }

        public async Task<List<SucursalDto>> ObtenerSucursalesAsync()
        {
            var empresaId = GetEmpresaId();
            if (string.IsNullOrEmpty(empresaId))
            {
                return new List<SucursalDto>();
            }

            var data = await _apiService.GetAsync<List<SucursalDto>>("/api/sucursales") ?? new List<SucursalDto>();
            return data.Where(s => s.EmpresaId == empresaId).ToList();
        }

        public async Task<bool> CrearSucursalAsync(SucursalDto sucursal)
        {
            var response = await _apiService.PostAsync<SucursalDto, SucursalDto>("/api/sucursales", sucursal);
            return response != null;
        }

        public async Task<bool> EliminarSucursalAsync(string sucursalId)
        {
            return await _apiService.DeleteAsync($"/api/sucursales/{sucursalId}");
        }
    }
}

