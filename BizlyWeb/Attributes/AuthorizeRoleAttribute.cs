using BizlyWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BizlyWeb.Attributes
{
    /// <summary>
    /// Atributo para autorizar acciones según el rol del usuario
    /// </summary>
    public class AuthorizeRoleAttribute : ActionFilterAttribute
    {
        private readonly string[] _allowedRoles;

        public AuthorizeRoleAttribute(params string[] allowedRoles)
        {
            _allowedRoles = allowedRoles;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var authService = context.HttpContext.RequestServices.GetRequiredService<AuthService>();
            
            if (!authService.IsAuthenticated())
            {
                context.Result = new RedirectToActionResult("Login", "Auth", new { returnUrl = context.HttpContext.Request.Path });
                return;
            }

            var userRole = authService.GetTipoUsuario();
            
            if (userRole == null || !_allowedRoles.Contains(userRole))
            {
                // Si es trabajador, redirigir a Ventas (punto de venta)
                // Si es emprendedor o no tiene rol, redirigir a Home
                if (userRole == "TRABAJADOR")
                {
                    context.Result = new RedirectToActionResult("Create", "Ventas", null);
                    context.HttpContext.Items["ErrorMessage"] = "No tienes permisos para acceder a esta sección. Solo puedes acceder a Ventas y Clientes.";
                }
                else
                {
                    context.Result = new RedirectToActionResult("Index", "Home", null);
                    context.HttpContext.Items["ErrorMessage"] = "No tienes permisos para acceder a esta sección.";
                }
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}


