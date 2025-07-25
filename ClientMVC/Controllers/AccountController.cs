using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using ClientMVC.Models;
using System.Diagnostics;
using System.Net.Http.Headers; // Added this line

namespace ClientMVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AccountController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult Login()
        {
            var token = HttpContext.Session.GetString("JWToken");
            if (!string.IsNullOrEmpty(token))
            {
                ViewData["TokenStatus"] = "Token JWT encontrado en la sesión.";
            }
            else
            {
                ViewData["TokenStatus"] = "No se encontró ningún Token JWT. Por favor, inicie sesión.";
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var client = _httpClientFactory.CreateClient("JWTAuthAPI");
            var json = JsonSerializer.Serialize(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("api/auth/login", content);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (tokenResponse?.AccessToken != null)
                {
                    HttpContext.Session.SetString("JWToken", tokenResponse.AccessToken);
                    ViewData["Username"] = tokenResponse.Username;
                    ViewData["Role"] = tokenResponse.Role;
                    return View("LoginSuccess"); 
                }
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        // Mover la acción Secret aquí si se desea mantenerla
        public async Task<IActionResult> Secret()
        {
            var token = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Account");
            }

            var client = _httpClientFactory.CreateClient("JWTAuthAPI");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync("api/secret");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                ViewData["SecretMessage"] = content;
            }
            else
            {
                ViewData["SecretMessage"] = $"Error: {response.StatusCode}";
            }

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public class TokenResponse
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public string? Username { get; set; }
        public string? Role { get; set; }
    }
}