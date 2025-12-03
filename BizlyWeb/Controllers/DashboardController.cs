using Microsoft.AspNetCore.Mvc;

namespace BizlyWeb.Controllers
{
    /// <summary>
    /// Controlador del Dashboard - Capa de Presentación
    /// </summary>
    public class DashboardController : Controller
    {
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(ILogger<DashboardController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Vista principal del Dashboard
        /// </summary>
        public IActionResult Index()
        {
            ViewData["Title"] = "Dashboard";
            return View();
        }
    }
}

