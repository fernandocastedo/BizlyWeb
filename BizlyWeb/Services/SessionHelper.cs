using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Http;

namespace BizlyWeb.Services
{
    /// <summary>
    /// Helper para obtener información de la sesión y del token JWT
    /// </summary>
    public static class SessionHelper
    {
        /// <summary>
        /// Obtiene el EmpresaId de la sesión o del token JWT
        /// </summary>
        public static string? GetEmpresaId(IHttpContextAccessor httpContextAccessor)
        {
            var session = httpContextAccessor.HttpContext?.Session;
            if (session == null) return null;

            // Intentar obtener de la sesión (primero como string, luego como int para compatibilidad)
            var empresaId = session.GetString("EmpresaId");
            if (!string.IsNullOrEmpty(empresaId))
            {
                return empresaId;
            }

            // Si no está en la sesión, intentar obtener del token JWT
            var token = session.GetString("JWTToken");
            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    var handler = new JwtSecurityTokenHandler();
                    var jsonToken = handler.ReadJwtToken(token);
                    var empresaIdClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == "empresaId");
                    if (empresaIdClaim != null && !string.IsNullOrEmpty(empresaIdClaim.Value))
                    {
                        // Guardar en la sesión para futuras consultas
                        session.SetString("EmpresaId", empresaIdClaim.Value);
                        return empresaIdClaim.Value;
                    }
                }
                catch
                {
                    // Si hay error al leer el token, continuar
                }
            }

            return null;
        }

        /// <summary>
        /// Obtiene el UsuarioId de la sesión o del token JWT
        /// </summary>
        public static string? GetUsuarioId(IHttpContextAccessor httpContextAccessor)
        {
            var session = httpContextAccessor.HttpContext?.Session;
            if (session == null) return null;

            // Intentar obtener de la sesión
            var usuarioId = session.GetString("UsuarioId");
            if (!string.IsNullOrEmpty(usuarioId))
            {
                return usuarioId;
            }

            // Si no está en la sesión, intentar obtener del token JWT
            var token = session.GetString("JWTToken");
            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    var handler = new JwtSecurityTokenHandler();
                    var jsonToken = handler.ReadJwtToken(token);
                    var subClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
                    if (subClaim != null && !string.IsNullOrEmpty(subClaim.Value))
                    {
                        // Guardar en la sesión para futuras consultas
                        session.SetString("UsuarioId", subClaim.Value);
                        return subClaim.Value;
                    }
                }
                catch
                {
                    // Si hay error al leer el token, continuar
                }
            }

            return null;
        }
    }
}


