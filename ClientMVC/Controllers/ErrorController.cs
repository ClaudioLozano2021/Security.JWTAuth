using Microsoft.AspNetCore.Mvc;

namespace ClientMVC.Controllers
{
    public class ErrorController : Controller
    {
        [HttpGet]
        public IActionResult Unauthorized()
        {
            ViewData["Title"] = "No Autorizado";
            return View();
        }

        [HttpGet] 
        public IActionResult TokenExpired()
        {
            ViewData["Title"] = "Sesión Expirada";
            return View();
        }
    }
}