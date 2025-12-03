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
                var apiResponse = await _apiService.PostAsync<LoginDto, LoginApiResponseDto>(
                    "/api/auth/login",
                    loginDto
                );

                if (apiResponse != null && !string.IsNullOrEmpty(apiResponse.Token) && apiResponse.Usuario != null)
                {
                    // Mapear la respuesta de la API al DTO interno
                    var response = new LoginResponseDto
                    {
                        Token = apiResponse.Token,
                        UsuarioId = apiResponse.Usuario.Id,
                        Nombre = apiResponse.Usuario.Nombre,
                        Email = apiResponse.Usuario.Email,
                        TipoUsuario = apiResponse.Usuario.TipoUsuario,
                        EmpresaId = apiResponse.Usuario.EmpresaId,
                        SucursalId = apiResponse.Usuario.SucursalId
                    };

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
                // La API devuelve un objeto con "usuario" anidado
                var apiResponse = await _apiService.PostAsync<RegisterDto, RegisterApiResponseDto>(
                    "/api/auth/registro-emprendedor",
                    registerDto
                );

                if (apiResponse != null && apiResponse.Usuario != null)
                {
                    // Después del registro, hacer login automático para obtener el token
                    var loginResponse = await LoginAsync(email, password);
                    
                    if (loginResponse != null)
                    {
                        return new RegisterResponseDto
                        {
                            Success = true,
                            Message = "Registro exitoso",
                            Usuario = loginResponse
                        };
                    }
                    else
                    {
                        // Si el login falla, al menos guardar los datos básicos
                        var usuarioResponse = new LoginResponseDto
                        {
                            Token = null,
                            UsuarioId = apiResponse.Usuario.Id,
                            Nombre = apiResponse.Usuario.Nombre,
                            Email = apiResponse.Usuario.Email,
                            TipoUsuario = apiResponse.Usuario.TipoUsuario,
                            EmpresaId = apiResponse.Usuario.EmpresaId,
                            SucursalId = apiResponse.Usuario.SucursalId
                        };

                        var session = _httpContextAccessor.HttpContext?.Session;
                        if (session != null)
                        {
                            session.SetString("TipoUsuario", usuarioResponse.TipoUsuario ?? "EMPRENDEDOR");
                            session.SetString("UserName", usuarioResponse.Nombre ?? string.Empty);
                            session.SetString("UserEmail", usuarioResponse.Email ?? string.Empty);
                            if (!string.IsNullOrEmpty(usuarioResponse.UsuarioId))
                            {
                                session.SetString("UsuarioId", usuarioResponse.UsuarioId);
                            }
                            if (!string.IsNullOrEmpty(usuarioResponse.EmpresaId))
                            {
                                session.SetString("EmpresaId", usuarioResponse.EmpresaId);
                            }
                            if (!string.IsNullOrEmpty(usuarioResponse.SucursalId))
                            {
                                session.SetString("SucursalId", usuarioResponse.SucursalId);
                            }
                        }

                        return new RegisterResponseDto
                        {
                            Success = true,
                            Message = "Registro exitoso. Por favor, inicia sesión.",
                            Usuario = usuarioResponse
                        };
                    }
                }

                return new RegisterResponseDto { Success = false, Message = "No se pudo completar el registro" };
            }
            catch
            {
                return new RegisterResponseDto { Success = false, Message = "Error al registrar" };
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
                
                if (!string.IsNullOrEmpty(usuario.UsuarioId))
                {
                    session.SetString("UsuarioId", usuario.UsuarioId);
                }
                
                if (!string.IsNullOrEmpty(usuario.EmpresaId))
                {
                    session.SetString("EmpresaId", usuario.EmpresaId);
                }

                if (!string.IsNullOrEmpty(usuario.SucursalId))
                {
                    session.SetString("SucursalId", usuario.SucursalId);
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
