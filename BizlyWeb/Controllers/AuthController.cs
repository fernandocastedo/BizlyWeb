using BizlyWeb.Models.ViewModels;
using BizlyWeb.Services;
using Microsoft.AspNetCore.Mvc;

namespace BizlyWeb.Controllers
{
    /// <summary>
    /// Controlador de Autenticación - Capa de Presentación
    /// </summary>
    public class AuthController : Controller
    {
        private readonly AuthService _authService;
        private readonly FileService _fileService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(AuthService authService, FileService fileService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _fileService = fileService;
            _logger = logger;
        }

        /// <summary>
        /// Muestra la vista de Login
        /// </summary>
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (_authService.IsAuthenticated())
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        /// <summary>
        /// Procesa el login del usuario
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
            {
                ModelState.AddModelError(string.Empty, "Email y contraseña son requeridos.");
                return View(model);
            }

            try
            {
                var response = await _authService.LoginAsync(model.Email, model.Password);

                if (response != null)
                {
                    TempData["SuccessMessage"] = $"¡Bienvenido, {response.Nombre ?? model.Email}!";
                    
                    if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }

                    return RedirectToAction("Index", "Dashboard");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Email o contraseña incorrectos.");
                    return View(model);
                }
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Error de negocio en login: {Message}", ex.Message);
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado en login: {Message}", ex.Message);
                ModelState.AddModelError(string.Empty, "Ocurrió un error al intentar iniciar sesión. Por favor, intente nuevamente.");
                return View(model);
            }
        }

        /// <summary>
        /// Muestra la vista de Registro
        /// </summary>
        [HttpGet]
        public IActionResult Register()
        {
            if (_authService.IsAuthenticated())
            {
                return RedirectToAction("Index", "Dashboard");
            }

            var pasoActual = HttpContext.Session.GetString("Registro_Paso") == "2" ? 2 : 1;
            var model = new RegisterViewModel { PasoActual = pasoActual };

            // Restaurar datos del paso 1 si existen
            if (pasoActual == 2)
            {
                model.NombreEmpresa = HttpContext.Session.GetString("Registro_NombreEmpresa") ?? string.Empty;
                model.Rubro = HttpContext.Session.GetString("Registro_Rubro") ?? string.Empty;
                model.Descripcion = HttpContext.Session.GetString("Registro_Descripcion");
                var margenStr = HttpContext.Session.GetString("Registro_MargenGanancia");
                if (decimal.TryParse(margenStr, out var margen))
                {
                    model.MargenGanancia = margen;
                }
            }

            return View(model);
        }

        /// <summary>
        /// Procesa el registro completo
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            // Paso 1: Validar datos del emprendimiento
            if (model.PasoActual == 1)
            {
                // Remover validación de campos del paso 2
                ModelState.Remove(nameof(model.Nombre));
                ModelState.Remove(nameof(model.Email));
                ModelState.Remove(nameof(model.Password));
                ModelState.Remove(nameof(model.ConfirmPassword));
                ModelState.Remove(nameof(model.Logo));

                if (!ModelState.IsValid)
                {
                    model.PasoActual = 1;
                    return View(model);
                }

                // Guardar datos del paso 1 en sesión
                HttpContext.Session.SetString("Registro_NombreEmpresa", model.NombreEmpresa);
                HttpContext.Session.SetString("Registro_Rubro", model.Rubro);
                HttpContext.Session.SetString("Registro_Descripcion", model.Descripcion ?? string.Empty);
                HttpContext.Session.SetString("Registro_MargenGanancia", model.MargenGanancia.ToString());

                // Procesar logo si existe
                string? logoUrlTemp = null;
                if (model.Logo != null && model.Logo.Length > 0)
                {
                    try
                    {
                        if (model.Logo.Length > 20 * 1024 * 1024)
                        {
                            ModelState.AddModelError(nameof(model.Logo), "El archivo es demasiado grande. El tamaño máximo es 20MB.");
                            model.PasoActual = 1;
                            return View(model);
                        }

                        logoUrlTemp = await _fileService.SaveLogoAsync(model.Logo);
                        if (!string.IsNullOrEmpty(logoUrlTemp))
                        {
                            HttpContext.Session.SetString("Registro_LogoUrl", logoUrlTemp);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error al guardar logo");
                        ModelState.AddModelError(nameof(model.Logo), "Error al procesar el logo. Puedes continuar sin logo.");
                        model.PasoActual = 1;
                        return View(model);
                    }
                }

                // Avanzar al paso 2
                HttpContext.Session.SetString("Registro_Paso", "2");
                model.PasoActual = 2;
                model.Logo = null; // Limpiar para no intentar procesarlo de nuevo
                return View(model);
            }

            // Paso 2: Validar datos del usuario y registrar
            if (string.IsNullOrEmpty(model.Nombre) || string.IsNullOrEmpty(model.Email) || 
                string.IsNullOrEmpty(model.Password) || string.IsNullOrEmpty(model.ConfirmPassword))
            {
                ModelState.AddModelError(string.Empty, "Por favor, complete todos los campos.");
                RestoreStepOneData(model);
                model.PasoActual = 2;
                return View(model);
            }

            if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError(nameof(model.ConfirmPassword), "Las contraseñas no coinciden.");
                RestoreStepOneData(model);
                model.PasoActual = 2;
                return View(model);
            }

            if (model.Password.Length < 6)
            {
                ModelState.AddModelError(nameof(model.Password), "La contraseña debe tener al menos 6 caracteres.");
                RestoreStepOneData(model);
                model.PasoActual = 2;
                return View(model);
            }

            // Recuperar datos del paso 1
            var nombreEmpresa = HttpContext.Session.GetString("Registro_NombreEmpresa") ?? model.NombreEmpresa;
            var rubro = HttpContext.Session.GetString("Registro_Rubro") ?? model.Rubro;
            var descripcion = HttpContext.Session.GetString("Registro_Descripcion");
            var margenStr = HttpContext.Session.GetString("Registro_MargenGanancia");
            var logoUrl = HttpContext.Session.GetString("Registro_LogoUrl");
            
            if (!decimal.TryParse(margenStr, out var margenGanancia))
            {
                margenGanancia = model.MargenGanancia;
            }

            try
            {
                var response = await _authService.RegisterAsync(
                    nombreEmpresa,
                    rubro,
                    descripcion,
                    margenGanancia,
                    logoUrl,
                    model.Nombre,
                    model.Email,
                    model.Password
                );

                if (response != null && response.Success)
                {
                    // Limpiar datos temporales
                    ClearRegistrationSession();
                    
                    TempData["SuccessMessage"] = "¡Registro exitoso! Bienvenido a Bizly.";
                    return RedirectToAction("Index", "Dashboard");
                }
                else
                {
                    var errorMessage = response?.Message ?? "Error al registrar el usuario. Por favor, intente nuevamente.";
                    ModelState.AddModelError(string.Empty, errorMessage);
                    RestoreStepOneData(model);
                    model.PasoActual = 2;
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar usuario {Email}", model.Email);
                ModelState.AddModelError(string.Empty, "Ha ocurrido un error al intentar registrarse. Por favor, intente nuevamente.");
                RestoreStepOneData(model);
                model.PasoActual = 2;
                return View(model);
            }
        }

        /// <summary>
        /// Restaura los datos del paso 1 en el modelo
        /// </summary>
        private void RestoreStepOneData(RegisterViewModel model)
        {
            model.NombreEmpresa = HttpContext.Session.GetString("Registro_NombreEmpresa") ?? model.NombreEmpresa;
            model.Rubro = HttpContext.Session.GetString("Registro_Rubro") ?? model.Rubro;
            model.Descripcion = HttpContext.Session.GetString("Registro_Descripcion");
            var margenStr = HttpContext.Session.GetString("Registro_MargenGanancia");
            if (decimal.TryParse(margenStr, out var margen))
            {
                model.MargenGanancia = margen;
            }
        }

        /// <summary>
        /// Limpia los datos temporales de registro de la sesión
        /// </summary>
        private void ClearRegistrationSession()
        {
            HttpContext.Session.Remove("Registro_Paso");
            HttpContext.Session.Remove("Registro_NombreEmpresa");
            HttpContext.Session.Remove("Registro_Rubro");
            HttpContext.Session.Remove("Registro_Descripcion");
            HttpContext.Session.Remove("Registro_MargenGanancia");
            HttpContext.Session.Remove("Registro_LogoUrl");
        }

        /// <summary>
        /// Cierra la sesión del usuario
        /// </summary>
        [HttpGet]
        [HttpPost]
        public IActionResult Logout()
        {
            _authService.Logout();
            TempData["InfoMessage"] = "Sesión cerrada correctamente.";
            return RedirectToAction("Login");
        }
    }
}
