using BizlyWeb.Models.DTOs;
using Microsoft.AspNetCore.Http;

namespace BizlyWeb.Services
{
    /// <summary>
    /// Servicio de negocio para administración de costos y gastos.
    /// Capa de Negocio
    /// </summary>
    public class CostoGastoService
    {
        private readonly ApiService _apiService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CostoGastoService(
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

        private string? GetUsuarioId()
        {
            return SessionHelper.GetUsuarioId(_httpContextAccessor);
        }

        /// <summary>
        /// Obtiene todos los costos y gastos de la empresa actual
        /// </summary>
        public async Task<List<CostoGastoDto>> ObtenerCostosGastosAsync()
        {
            var empresaId = GetEmpresaId();
            if (string.IsNullOrEmpty(empresaId))
            {
                return new List<CostoGastoDto>();
            }

            var data = await _apiService.GetAsync<List<CostoGastoDto>>("/api/costosgastos") ?? new List<CostoGastoDto>();
            return data.Where(cg => cg.EmpresaId == empresaId).ToList();
        }

        /// <summary>
        /// Obtiene un costo/gasto por ID
        /// </summary>
        public async Task<CostoGastoDto?> ObtenerCostoGastoPorIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            return await _apiService.GetAsync<CostoGastoDto>($"/api/costosgastos/{id}");
        }

        /// <summary>
        /// Crea un nuevo costo/gasto
        /// </summary>
        public async Task<CostoGastoDto?> CrearCostoGastoAsync(CostoGastoDto costoGasto)
        {
            var empresaId = GetEmpresaId();
            var usuarioId = GetUsuarioId();
            if (string.IsNullOrEmpty(empresaId) || string.IsNullOrEmpty(usuarioId))
            {
                throw new InvalidOperationException("No se pudo obtener la empresa o usuario actual.");
            }

            costoGasto.EmpresaId = empresaId;
            costoGasto.UsuarioId = usuarioId;
            costoGasto.CreatedAt = DateTime.UtcNow;

            return await _apiService.PostAsync<CostoGastoDto, CostoGastoDto>("/api/costosgastos", costoGasto);
        }

        /// <summary>
        /// Actualiza un costo/gasto existente
        /// </summary>
        public async Task<bool> ActualizarCostoGastoAsync(string id, CostoGastoDto costoGasto)
        {
            if (string.IsNullOrEmpty(id) || costoGasto == null)
            {
                return false;
            }

            var empresaId = GetEmpresaId();
            var usuarioId = GetUsuarioId();
            if (string.IsNullOrEmpty(empresaId) || string.IsNullOrEmpty(usuarioId))
            {
                throw new InvalidOperationException("No se pudo obtener la empresa o usuario actual.");
            }

            costoGasto.EmpresaId = empresaId;
            costoGasto.UsuarioId = usuarioId;
            costoGasto.Id = id;

            try
            {
                await _apiService.PutAsync<CostoGastoDto, CostoGastoDto>($"/api/costosgastos/{id}", costoGasto);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Elimina un costo/gasto
        /// </summary>
        public async Task<bool> EliminarCostoGastoAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return false;
            }

            try
            {
                await _apiService.DeleteAsync($"/api/costosgastos/{id}");
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Obtiene costos/gastos filtrados (RF-36)
        /// </summary>
        public async Task<List<CostoGastoDto>> ObtenerCostosGastosFiltradosAsync(
            DateTime? fechaInicio = null,
            DateTime? fechaFin = null,
            string? categoriaFinanciera = null,
            string? clasificacion = null,
            string? sucursalId = null)
        {
            var costosGastos = await ObtenerCostosGastosAsync();

            if (fechaInicio.HasValue)
            {
                costosGastos = costosGastos.Where(cg => cg.Fecha >= fechaInicio.Value).ToList();
            }

            if (fechaFin.HasValue)
            {
                costosGastos = costosGastos.Where(cg => cg.Fecha <= fechaFin.Value.AddDays(1).AddSeconds(-1)).ToList();
            }

            if (!string.IsNullOrEmpty(categoriaFinanciera))
            {
                costosGastos = costosGastos.Where(cg => cg.CategoriaFinanciera == categoriaFinanciera).ToList();
            }

            if (!string.IsNullOrEmpty(clasificacion))
            {
                costosGastos = costosGastos.Where(cg => cg.Clasificacion == clasificacion).ToList();
            }

            if (!string.IsNullOrEmpty(sucursalId))
            {
                costosGastos = costosGastos.Where(cg => cg.SucursalId == sucursalId).ToList();
            }

            return costosGastos.OrderByDescending(cg => cg.Fecha).ToList();
        }

        /// <summary>
        /// Detecta incrementos significativos de costos (RF-37)
        /// Compara el mes actual con el mes anterior
        /// </summary>
        public async Task<List<(string Categoria, decimal MontoAnterior, decimal MontoActual, decimal PorcentajeIncremento)>> DetectarIncrementosCostosAsync()
        {
            var ahora = DateTime.UtcNow;
            var inicioMesActual = new DateTime(ahora.Year, ahora.Month, 1);
            var finMesActual = inicioMesActual.AddMonths(1).AddSeconds(-1);
            var inicioMesAnterior = inicioMesActual.AddMonths(-1);
            var finMesAnterior = inicioMesActual.AddSeconds(-1);

            var costosMesActual = await ObtenerCostosGastosFiltradosAsync(inicioMesActual, finMesActual);
            var costosMesAnterior = await ObtenerCostosGastosFiltradosAsync(inicioMesAnterior, finMesAnterior);

            var incrementos = new List<(string Categoria, decimal MontoAnterior, decimal MontoActual, decimal PorcentajeIncremento)>();

            // Agrupar por categoría financiera
            var categorias = new[] { "DIRECTO", "ADMINISTRATIVO" };
            foreach (var categoria in categorias)
            {
                var montoActual = costosMesActual
                    .Where(cg => cg.CategoriaFinanciera == categoria)
                    .Sum(cg => cg.Monto);

                var montoAnterior = costosMesAnterior
                    .Where(cg => cg.CategoriaFinanciera == categoria)
                    .Sum(cg => cg.Monto);

                if (montoAnterior > 0)
                {
                    var porcentajeIncremento = ((montoActual - montoAnterior) / montoAnterior) * 100;
                    // Alertar si el incremento es mayor al 20%
                    if (porcentajeIncremento > 20)
                    {
                        incrementos.Add((categoria, montoAnterior, montoActual, porcentajeIncremento));
                    }
                }
                else if (montoActual > 0 && montoAnterior == 0)
                {
                    // Si no había costos el mes anterior y ahora sí hay, es un incremento del 100%
                    incrementos.Add((categoria, 0, montoActual, 100));
                }
            }

            return incrementos;
        }
    }
}


