using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace BizlyWeb.Filters
{
    /// <summary>
    /// Filtro para manejar excepciones de la API de manera centralizada
    /// </summary>
    public class ApiExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<ApiExceptionFilter> _logger;

        public ApiExceptionFilter(ILogger<ApiExceptionFilter> logger)
        {
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            _logger.LogError(context.Exception, "Error no manejado: {Message}", context.Exception.Message);

            // Si la excepción es de tipo HttpRequestException (errores de API)
            if (context.Exception is HttpRequestException httpEx)
            {
                context.Result = new RedirectToActionResult(
                    "Error",
                    "Home",
                    new { message = "Error al comunicarse con el servidor. Por favor, intente nuevamente." });
                context.ExceptionHandled = true;
                return;
            }

            // Para otras excepciones, mostrar página de error genérica
            if (!context.ExceptionHandled)
            {
                context.Result = new RedirectToActionResult(
                    "Error",
                    "Home",
                    new { message = "Ha ocurrido un error inesperado. Por favor, contacte al administrador." });
                context.ExceptionHandled = true;
            }
        }
    }
}


