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

                if (apiResponse == null)
                {
                    throw new InvalidOperationException("La API no devolvió una respuesta válida.");
                }

                if (string.IsNullOrEmpty(apiResponse.Token))
                {
                    throw new InvalidOperationException("La API no devolvió un token válido.");
                }

                if (apiResponse.Usuario == null)
                {
                    throw new InvalidOperationException("La API no devolvió información del usuario.");
                }

                // Validar que el EmpresaId no esté vacío
                if (string.IsNullOrEmpty(apiResponse.Usuario.EmpresaId))
                {
                    throw new InvalidOperationException("El usuario no tiene una empresa asociada. Contacta al administrador.");
                }

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
                
                // Verificar que se guardó correctamente
                var session = _httpContextAccessor.HttpContext?.Session;
                var empresaIdGuardado = session?.GetString("EmpresaId");
                if (string.IsNullOrEmpty(empresaIdGuardado))
                {
                    // Intentar obtener del token JWT como respaldo
                    empresaIdGuardado = SessionHelper.GetEmpresaId(_httpContextAccessor);
                    if (string.IsNullOrEmpty(empresaIdGuardado))
                    {
                        throw new InvalidOperationException("Error al guardar el EmpresaId en la sesión. EmpresaId recibido: " + apiResponse.Usuario.EmpresaId);
                    }
                }

                return response;
            }
            catch (Exception ex)
            {
                // Log del error para debugging
                var logger = _httpContextAccessor.HttpContext?.RequestServices.GetService<ILogger<AuthService>>();
                logger?.LogError(ex, "Error en LoginAsync: {Message}", ex.Message);
                throw;
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
                // Asegurar que la sesión esté disponible
                session.SetString("JWTToken", usuario.Token);
                session.SetString("TipoUsuario", usuario.TipoUsuario ?? "EMPRENDEDOR");
                session.SetString("UserName", usuario.Nombre ?? string.Empty);
                session.SetString("UserEmail", usuario.Email ?? string.Empty);
                
                if (!string.IsNullOrEmpty(usuario.UsuarioId))
                {
                    session.SetString("UsuarioId", usuario.UsuarioId);
                }
                
                // CRÍTICO: Guardar EmpresaId como string
                if (!string.IsNullOrEmpty(usuario.EmpresaId))
                {
                    session.SetString("EmpresaId", usuario.EmpresaId);
                }
                else
                {
                    throw new InvalidOperationException("El EmpresaId no puede estar vacío. El usuario debe tener una empresa asociada.");
                }

                if (!string.IsNullOrEmpty(usuario.SucursalId))
                {
                    session.SetString("SucursalId", usuario.SucursalId);
                }

                // Forzar commit de la sesión (opcional, ASP.NET Core lo hace automáticamente)
                // session.CommitAsync().Wait(); // Comentado porque puede causar deadlocks
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
