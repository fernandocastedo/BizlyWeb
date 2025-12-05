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

            var pasoStr = HttpContext.Session.GetString("Registro_Paso");
            var pasoActual = pasoStr == "2" ? 2 : pasoStr == "3" ? 3 : 1;
            var model = new RegisterViewModel { PasoActual = pasoActual };

            // Restaurar datos del paso 1 si existen
            if (pasoActual >= 2)
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

            // Restaurar datos del paso 2 si existen
            if (pasoActual == 3)
            {
                model.Nombre = HttpContext.Session.GetString("Registro_Nombre") ?? string.Empty;
                model.Email = HttpContext.Session.GetString("Registro_Email") ?? string.Empty;
            }

            // Restaurar datos del paso 3 si existen
            if (pasoActual == 3)
            {
                model.NombreSucursal = HttpContext.Session.GetString("Registro_NombreSucursal");
                model.DireccionSucursal = HttpContext.Session.GetString("Registro_DireccionSucursal");
                model.CiudadSucursal = HttpContext.Session.GetString("Registro_CiudadSucursal");
                model.DepartamentoSucursal = HttpContext.Session.GetString("Registro_DepartamentoSucursal");
                model.TelefonoSucursal = HttpContext.Session.GetString("Registro_TelefonoSucursal");
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
                // Remover validación de campos de otros pasos
                ModelState.Remove(nameof(model.Nombre));
                ModelState.Remove(nameof(model.Email));
                ModelState.Remove(nameof(model.Password));
                ModelState.Remove(nameof(model.ConfirmPassword));
                ModelState.Remove(nameof(model.Logo));
                ModelState.Remove(nameof(model.NombreSucursal));
                ModelState.Remove(nameof(model.DireccionSucursal));
                ModelState.Remove(nameof(model.CiudadSucursal));
                ModelState.Remove(nameof(model.DepartamentoSucursal));
                ModelState.Remove(nameof(model.TelefonoSucursal));

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
                RestoreStepOneData(model);
                return View(model);
            }

            // Paso 2: Validar datos del usuario
            if (model.PasoActual == 2)
            {
                // Remover validación de campos de otros pasos
                ModelState.Remove(nameof(model.NombreEmpresa));
                ModelState.Remove(nameof(model.Rubro));
                ModelState.Remove(nameof(model.Descripcion));
                ModelState.Remove(nameof(model.MargenGanancia));
                ModelState.Remove(nameof(model.Logo));
                ModelState.Remove(nameof(model.NombreSucursal));
                ModelState.Remove(nameof(model.DireccionSucursal));
                ModelState.Remove(nameof(model.CiudadSucursal));
                ModelState.Remove(nameof(model.DepartamentoSucursal));
                ModelState.Remove(nameof(model.TelefonoSucursal));

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

                // Guardar datos del paso 2 en sesión
                HttpContext.Session.SetString("Registro_Nombre", model.Nombre);
                HttpContext.Session.SetString("Registro_Email", model.Email);
                HttpContext.Session.SetString("Registro_Password", model.Password); // Solo para validación, no se guarda permanentemente

                // Avanzar al paso 3
                HttpContext.Session.SetString("Registro_Paso", "3");
                model.PasoActual = 3;
                RestoreStepOneData(model);
                RestoreStepTwoData(model);
                return View(model);
            }

            // Paso 3: Validar datos de sucursal (opcional) y registrar
            if (model.PasoActual == 3)
            {
                // Remover validación de campos de otros pasos
                ModelState.Remove(nameof(model.NombreEmpresa));
                ModelState.Remove(nameof(model.Rubro));
                ModelState.Remove(nameof(model.Descripcion));
                ModelState.Remove(nameof(model.MargenGanancia));
                ModelState.Remove(nameof(model.Logo));
                ModelState.Remove(nameof(model.Nombre));
                ModelState.Remove(nameof(model.Email));
                ModelState.Remove(nameof(model.Password));
                ModelState.Remove(nameof(model.ConfirmPassword));

                // Guardar datos de sucursal en sesión (opcional)
                if (!string.IsNullOrWhiteSpace(model.NombreSucursal))
                {
                    HttpContext.Session.SetString("Registro_NombreSucursal", model.NombreSucursal);
                }
                if (!string.IsNullOrWhiteSpace(model.DireccionSucursal))
                {
                    HttpContext.Session.SetString("Registro_DireccionSucursal", model.DireccionSucursal);
                }
                if (!string.IsNullOrWhiteSpace(model.CiudadSucursal))
                {
                    HttpContext.Session.SetString("Registro_CiudadSucursal", model.CiudadSucursal);
                }
                if (!string.IsNullOrWhiteSpace(model.DepartamentoSucursal))
                {
                    HttpContext.Session.SetString("Registro_DepartamentoSucursal", model.DepartamentoSucursal);
                }
                if (!string.IsNullOrWhiteSpace(model.TelefonoSucursal))
                {
                    HttpContext.Session.SetString("Registro_TelefonoSucursal", model.TelefonoSucursal);
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
                
                // Validar que los campos requeridos no estén vacíos
                if (string.IsNullOrWhiteSpace(nombreEmpresa))
                {
                    ModelState.AddModelError(string.Empty, "El nombre del emprendimiento es requerido.");
                    RestoreStepOneData(model);
                    RestoreStepTwoData(model);
                    model.PasoActual = 3;
                    return View(model);
                }
                
                if (string.IsNullOrWhiteSpace(rubro))
                {
                    ModelState.AddModelError(string.Empty, "El rubro es requerido.");
                    RestoreStepOneData(model);
                    RestoreStepTwoData(model);
                    model.PasoActual = 3;
                    return View(model);
                }
                
                // Recuperar datos del paso 2
                var nombreUsuario = HttpContext.Session.GetString("Registro_Nombre") ?? model.Nombre;
                var email = HttpContext.Session.GetString("Registro_Email") ?? model.Email;
                var password = HttpContext.Session.GetString("Registro_Password") ?? model.Password;

                if (string.IsNullOrWhiteSpace(nombreUsuario) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                {
                    ModelState.AddModelError(string.Empty, "Los datos del usuario son requeridos.");
                    RestoreStepOneData(model);
                    RestoreStepTwoData(model);
                    model.PasoActual = 3;
                    return View(model);
                }
                
                // La API requiere que DescripcionEmpresa no esté vacío
                if (string.IsNullOrWhiteSpace(descripcion))
                {
                    descripcion = "Sin descripción";
                }
                
                // La API requiere que MargenGanancia sea mayor a 0
                if (margenGanancia <= 0)
                {
                    margenGanancia = 30; // Valor por defecto
                }

                // Recuperar datos de sucursal (opcional)
                var nombreSucursal = HttpContext.Session.GetString("Registro_NombreSucursal") ?? model.NombreSucursal;
                var direccionSucursal = HttpContext.Session.GetString("Registro_DireccionSucursal") ?? model.DireccionSucursal;
                var ciudadSucursal = HttpContext.Session.GetString("Registro_CiudadSucursal") ?? model.CiudadSucursal;
                var departamentoSucursal = HttpContext.Session.GetString("Registro_DepartamentoSucursal") ?? model.DepartamentoSucursal;

                try
                {
                    var response = await _authService.RegisterAsync(
                        nombreEmpresa,
                        rubro,
                        descripcion,
                        margenGanancia,
                        logoUrl,
                        nombreUsuario,
                        email,
                        password,
                        nombreSucursal,
                        direccionSucursal,
                        ciudadSucursal,
                        departamentoSucursal
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
                    RestoreStepTwoData(model);
                    model.PasoActual = 3;
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar usuario {Email}", model.Email);
                ModelState.AddModelError(string.Empty, "Ha ocurrido un error al intentar registrarse. Por favor, intente nuevamente.");
                RestoreStepOneData(model);
                RestoreStepTwoData(model);
                model.PasoActual = 3;
                return View(model);
            }
            }

            // Si llegamos aquí, el paso actual no es válido
            model.PasoActual = 1;
            return View(model);
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
        /// Restaura los datos del paso 2 en el modelo
        /// </summary>
        private void RestoreStepTwoData(RegisterViewModel model)
        {
            model.Nombre = HttpContext.Session.GetString("Registro_Nombre") ?? model.Nombre;
            model.Email = HttpContext.Session.GetString("Registro_Email") ?? model.Email;
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
            HttpContext.Session.Remove("Registro_Nombre");
            HttpContext.Session.Remove("Registro_Email");
            HttpContext.Session.Remove("Registro_Password");
            HttpContext.Session.Remove("Registro_NombreSucursal");
            HttpContext.Session.Remove("Registro_DireccionSucursal");
            HttpContext.Session.Remove("Registro_CiudadSucursal");
            HttpContext.Session.Remove("Registro_DepartamentoSucursal");
            HttpContext.Session.Remove("Registro_TelefonoSucursal");
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
