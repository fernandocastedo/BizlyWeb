using BizlyWeb.Models.DTOs;
using Microsoft.AspNetCore.Http;

namespace BizlyWeb.Services
{
    /// <summary>
    /// Servicio de negocio para administración de trabajadores.
    /// Capa de Negocio
    /// </summary>
    public class TrabajadorService
    {
        private readonly ApiService _apiService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly VentaService _ventaService;

        public TrabajadorService(
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
        /// Obtiene todos los trabajadores de la empresa actual
        /// </summary>
        public async Task<List<TrabajadorDto>> ObtenerTrabajadoresAsync()
        {
            var empresaId = GetEmpresaId();
            if (string.IsNullOrEmpty(empresaId))
            {
                return new List<TrabajadorDto>();
            }

            var data = await _apiService.GetAsync<List<TrabajadorDto>>("/api/trabajadores") ?? new List<TrabajadorDto>();
            return data.Where(t => t.EmpresaId == empresaId).ToList();
        }

        /// <summary>
        /// Obtiene un trabajador por ID
        /// </summary>
        public async Task<TrabajadorDto?> ObtenerTrabajadorPorIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            return await _apiService.GetAsync<TrabajadorDto>($"/api/trabajadores/{id}");
        }

        /// <summary>
        /// Crea un nuevo trabajador
        /// </summary>
        public async Task<TrabajadorDto?> CrearTrabajadorAsync(TrabajadorDto trabajador)
        {
            var empresaId = GetEmpresaId();
            if (string.IsNullOrEmpty(empresaId))
            {
                throw new InvalidOperationException("No se pudo obtener la empresa actual.");
            }

            trabajador.EmpresaId = empresaId;
            trabajador.CreatedAt = DateTime.UtcNow;
            trabajador.UpdatedAt = DateTime.UtcNow;

            return await _apiService.PostAsync<TrabajadorDto, TrabajadorDto>("/api/trabajadores", trabajador);
        }

        /// <summary>
        /// Actualiza un trabajador existente
        /// </summary>
        public async Task<bool> ActualizarTrabajadorAsync(string id, TrabajadorDto trabajador)
        {
            if (string.IsNullOrEmpty(id) || trabajador == null)
            {
                return false;
            }

            var empresaId = GetEmpresaId();
            if (string.IsNullOrEmpty(empresaId))
            {
                throw new InvalidOperationException("No se pudo obtener la empresa actual.");
            }

            trabajador.EmpresaId = empresaId;
            trabajador.Id = id;
            trabajador.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _apiService.PutAsync<TrabajadorDto, TrabajadorDto>($"/api/trabajadores/{id}", trabajador);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Elimina un trabajador
        /// </summary>
        public async Task<bool> EliminarTrabajadorAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return false;
            }

            try
            {
                await _apiService.DeleteAsync($"/api/trabajadores/{id}");
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Crea un usuario vinculado al trabajador (RF-48)
        /// </summary>
        public async Task<bool> CrearUsuarioTrabajadorAsync(string trabajadorId, string nombre, string email, string password, string? sucursalId = null)
        {
            if (string.IsNullOrEmpty(trabajadorId) || string.IsNullOrEmpty(nombre) || 
                string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                return false;
            }

            try
            {
                var request = new
                {
                    trabajadorId = trabajadorId,
                    nombre = nombre,
                    email = email,
                    password = password,
                    sucursalId = sucursalId
                };

                var response = await _apiService.PostAsync<object, object>("/api/auth/crear-trabajador", request);
                return response != null;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Desactiva el acceso de un trabajador (RF-52)
        /// Busca el usuario asociado al trabajador y lo desactiva
        /// </summary>
        public async Task<bool> DesactivarAccesoTrabajadorAsync(string trabajadorId)
        {
            if (string.IsNullOrEmpty(trabajadorId))
            {
                return false;
            }

            try
            {
                // Obtener todos los usuarios de la empresa
                var usuarios = await _apiService.GetAsync<List<UsuarioApiDto>>("/api/usuarios") ?? new List<UsuarioApiDto>();
                var empresaId = GetEmpresaId();
                
                // Buscar el usuario que tenga el TrabajadorId
                var usuarioTrabajador = usuarios
                    .FirstOrDefault(u => u.TrabajadorId == trabajadorId && u.EmpresaId == empresaId);

                if (usuarioTrabajador == null || string.IsNullOrEmpty(usuarioTrabajador.Id))
                {
                    return false; // No hay usuario asociado
                }

                // Actualizar el usuario para desactivarlo
                // Nota: Asumiendo que hay un endpoint PUT /api/usuarios/{id} que permite actualizar el campo Activo
                // Si no existe, necesitaríamos crear un endpoint específico
                var usuarioUpdate = new
                {
                    id = usuarioTrabajador.Id,
                    activo = false
                };

                try
                {
                    await _apiService.PutAsync<object, object>($"/api/usuarios/{usuarioTrabajador.Id}", usuarioUpdate);
                    return true;
                }
                catch
                {
                    // Si falla, intentar con un endpoint específico si existe
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Obtiene el reporte de desempeño de un trabajador (RF-51)
        /// Basado en las ventas realizadas por el usuario asociado al trabajador
        /// </summary>
        public async Task<(int TotalVentas, decimal TotalIngresos, decimal PromedioVenta, int VentasMesActual, decimal IngresosMesActual)?> ObtenerDesempenoTrabajadorAsync(string trabajadorId, DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            if (string.IsNullOrEmpty(trabajadorId))
            {
                return null;
            }

            try
            {
                // Obtener el usuario asociado al trabajador
                var usuarios = await _apiService.GetAsync<List<UsuarioApiDto>>("/api/usuarios") ?? new List<UsuarioApiDto>();
                var empresaId = GetEmpresaId();
                
                var usuarioTrabajador = usuarios
                    .FirstOrDefault(u => u.TrabajadorId == trabajadorId && u.EmpresaId == empresaId);

                if (usuarioTrabajador == null || string.IsNullOrEmpty(usuarioTrabajador.Id))
                {
                    return null; // No hay usuario asociado, no hay ventas
                }

                // Obtener todas las ventas
                var ventas = await _ventaService.ObtenerVentasFiltradasAsync(fechaInicio, fechaFin, estadoPedido: "completado");
                
                // Filtrar ventas del usuario trabajador
                var ventasTrabajador = ventas
                    .Where(v => v.UsuarioId == usuarioTrabajador.Id && v.EstadoPedido == "completado")
                    .ToList();

                if (!ventasTrabajador.Any())
                {
                    return (0, 0, 0, 0, 0);
                }

                var totalVentas = ventasTrabajador.Count;
                var totalIngresos = ventasTrabajador.Sum(v => v.Total);
                var promedioVenta = totalIngresos / totalVentas;

                // Calcular ventas del mes actual
                var ahora = DateTime.UtcNow;
                var inicioMesActual = new DateTime(ahora.Year, ahora.Month, 1);
                var finMesActual = inicioMesActual.AddMonths(1).AddSeconds(-1);
                
                var ventasMesActual = ventasTrabajador
                    .Where(v => v.Fecha >= inicioMesActual && v.Fecha <= finMesActual)
                    .ToList();

                var ventasMesActualCount = ventasMesActual.Count;
                var ingresosMesActual = ventasMesActual.Sum(v => v.Total);

                return (totalVentas, totalIngresos, promedioVenta, ventasMesActualCount, ingresosMesActual);
            }
            catch
            {
                return null;
            }
        }
    }
}


