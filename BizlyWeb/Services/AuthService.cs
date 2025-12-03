using BizlyWeb.Models.DTOs;
using Microsoft.AspNetCore.Http;

namespace BizlyWeb.Services
{
    /// <summary>
    /// Servicio de autenticación - Capa de Negocio
    /// Maneja la lógica de negocio para login y registro
    /// </summary>
    public class AuthService
    {
        private readonly ApiService _apiService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(ApiService apiService, IHttpContextAccessor httpContextAccessor)
        {
            _apiService = apiService;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Realiza el login del usuario
        /// </summary>
        public async Task<LoginResponseDto?> LoginAsync(string email, string password)
        {
            var loginDto = new LoginDto
            {
                Email = email,
                Password = password
            };

            try
            {
                var response = await _apiService.PostAsync<LoginDto, LoginResponseDto>(
                    "/api/auth/login",
                    loginDto
                );

                if (response != null && !string.IsNullOrEmpty(response.Token))
                {
                    SetUserSession(response);
                    return response;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Realiza el registro de un nuevo emprendedor
        /// </summary>
        public async Task<RegisterResponseDto?> RegisterAsync(
            string nombreEmpresa,
            string rubro,
            string? descripcion,
            decimal margenGanancia,
            string? logoUrl,
            string nombreUsuario,
            string email,
            string password)
        {
            var registerDto = new RegisterDto
            {
                NombreEmpresa = nombreEmpresa,
                Rubro = rubro,
                DescripcionEmpresa = descripcion ?? string.Empty,
                MargenGanancia = margenGanancia,
                LogoUrl = logoUrl ?? string.Empty,
                NombreUsuario = nombreUsuario,
                Email = email,
                Password = password
            };

            try
            {
                var response = await _apiService.PostAsync<RegisterDto, RegisterResponseDto>(
                    "/api/auth/registro-emprendedor",
                    registerDto
                );

                if (response != null && response.Success && response.Usuario != null)
                {
                    SetUserSession(response.Usuario);
                }

                return response;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Guarda los datos del usuario en la sesión
        /// </summary>
        private void SetUserSession(LoginResponseDto usuario)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session != null && !string.IsNullOrEmpty(usuario.Token))
            {
                session.SetString("JWTToken", usuario.Token);
                session.SetString("TipoUsuario", usuario.TipoUsuario ?? "EMPRENDEDOR");
                session.SetString("UserName", usuario.Nombre ?? string.Empty);
                session.SetString("UserEmail", usuario.Email ?? string.Empty);
                
                if (usuario.UsuarioId.HasValue)
                {
                    session.SetInt32("UsuarioId", usuario.UsuarioId.Value);
                }
                
                if (usuario.EmpresaId.HasValue)
                {
                    session.SetInt32("EmpresaId", usuario.EmpresaId.Value);
                }
            }
        }

        /// <summary>
        /// Verifica si el usuario está autenticado
        /// </summary>
        public bool IsAuthenticated()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            return session != null && !string.IsNullOrEmpty(session.GetString("JWTToken"));
        }

        /// <summary>
        /// Obtiene el tipo de usuario actual
        /// </summary>
        public string? GetTipoUsuario()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            return session?.GetString("TipoUsuario");
        }

        /// <summary>
        /// Verifica si el usuario es EMPRENDEDOR
        /// </summary>
        public bool IsEmprendedor()
        {
            return GetTipoUsuario() == "EMPRENDEDOR";
        }

        /// <summary>
        /// Verifica si el usuario es TRABAJADOR
        /// </summary>
        public bool IsTrabajador()
        {
            return GetTipoUsuario() == "TRABAJADOR";
        }

        /// <summary>
        /// Cierra la sesión del usuario
        /// </summary>
        public void Logout()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session != null)
            {
                session.Clear();
            }
        }
    }
}
