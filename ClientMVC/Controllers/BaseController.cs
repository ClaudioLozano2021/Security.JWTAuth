using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ClientMVC.Services;

namespace ClientMVC.Controllers
{
    public class BaseController : Controller
    {
        protected readonly ITokenService _tokenService;
        protected readonly IHttpClientFactory _httpClientFactory;

        public BaseController(ITokenService tokenService, IHttpClientFactory httpClientFactory)
        {
            _tokenService = tokenService;
            _httpClientFactory = httpClientFactory;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            Console.WriteLine($"BaseController - Usuario autenticado: {User.Identity?.IsAuthenticated}");
            Console.WriteLine($"BaseController - Nombre usuario: {User.Identity?.Name}");
            
            // Check if user is authenticated and token is valid
            if (User.Identity?.IsAuthenticated == true)
            {
                if (!_tokenService.IsTokenValid(HttpContext))
                {
                    Console.WriteLine("Token inválido, redirigiendo a página de sesión expirada");
                    context.Result = RedirectToAction("TokenExpired", "Error");
                    return;
                }

                // Store token info in ViewData for views
                ViewData["Username"] = HttpContext.Session.GetString("Username");
                ViewData["Role"] = HttpContext.Session.GetString("Role");
                Console.WriteLine($"BaseController - ViewData configurado para {ViewData["Username"]}");
            }
            else
            {
                Console.WriteLine("Usuario no autenticado en BaseController");
            }

            base.OnActionExecuting(context);
        }

        protected HttpClient CreateAuthorizedClient(string clientName)
        {
            var client = _httpClientFactory.CreateClient(clientName);
            var token = _tokenService.GetToken(HttpContext);
            
            Console.WriteLine($"CreateAuthorizedClient - Cliente: {clientName}");
            Console.WriteLine($"CreateAuthorizedClient - Token: {(string.IsNullOrEmpty(token) ? "NULL" : token.Substring(0, Math.Min(50, token.Length)) + "...")}");
            
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                Console.WriteLine($"CreateAuthorizedClient - Header Authorization configurado");
            }
            else
            {
                Console.WriteLine("CreateAuthorizedClient - NO se pudo configurar Authorization header (token vacío)");
            }

            return client;
        }
    }
}