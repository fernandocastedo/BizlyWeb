using BizlyWeb.Services;

namespace BizlyWeb.Middleware
{
    /// <summary>
    /// Middleware para validar autenticación y redirigir a login si es necesario
    /// </summary>
    public class AuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuthMiddleware> _logger;

        // Rutas públicas que no requieren autenticación
        private readonly string[] _publicRoutes =
        {
            "/",
            "/auth/login",
            "/auth/register",
            "/home/error"
        };

        private readonly string[] _staticResourcePrefixes =
        {
            "/css",
            "/js",
            "/lib",
            "/favicon",
            "/uploads",
            "/images",
            "/.well-known" // usado por devtools
        };

        public AuthMiddleware(RequestDelegate next, ILogger<AuthMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, AuthService authService)
        {
            try
            {
                var path = context.Request.Path.Value?.ToLower() ?? "";

                // Verificar si la ruta es pública
                bool isPublicRoute = _publicRoutes.Any(route => path.Equals(route, StringComparison.OrdinalIgnoreCase));

                // Rutas de recursos estáticos o prefijos permitidos (css, js, devtools, etc.)
                bool isStaticResource = _staticResourcePrefixes.Any(prefix =>
                    context.Request.Path.StartsWithSegments(prefix, StringComparison.OrdinalIgnoreCase));

                // Verificar autenticación solo si la sesión está disponible
                bool isAuthenticated = false;
                try
                {
                    isAuthenticated = authService.IsAuthenticated();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error al verificar autenticación en middleware para path {Path}", path);
                    // Si hay error, tratar como no autenticado
                    isAuthenticated = false;
                }

                // Si no es una ruta pública y el usuario no está autenticado, redirigir a login
                if (!isPublicRoute && !isStaticResource && !isAuthenticated)
                {
                    _logger.LogWarning("Usuario no autenticado intentando acceder a {Path}", path);
                    
                    // Evitar redirección infinita
                    if (!context.Response.HasStarted)
                    {
                        var returnUrl = Uri.EscapeDataString(context.Request.Path + context.Request.QueryString);
                        context.Response.Redirect($"/Auth/Login?returnUrl={returnUrl}");
                        return;
                    }
                }

                // Si está autenticado y trata de acceder a Login o Register, redirigir al dashboard
                if (isAuthenticated && (path.Contains("/auth/login") || path.Contains("/auth/register")))
                {
                    if (!context.Response.HasStarted)
                    {
                        context.Response.Redirect("/Dashboard");
                        return;
                    }
                }

                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en AuthMiddleware: {Message}", ex.Message);
                // Continuar con el pipeline para que otros middlewares puedan manejar el error
                await _next(context);
            }
        }
    }

    /// <summary>
    /// Extensión para registrar el middleware
    /// </summary>
    public static class AuthMiddlewareExtensions
    {
        public static IApplicationBuilder UseAuthMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthMiddleware>();
        }
    }
}

