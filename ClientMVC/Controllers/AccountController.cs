using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using ClientMVC.Models;
using System.Diagnostics;
using System.Net.Http.Headers;
using ClientMVC.Services;
using Microsoft.AspNetCore.Authorization;

namespace ClientMVC.Controllers
{
    public class AccountController : BaseController
    {
        public AccountController(ITokenService tokenService, IHttpClientFactory httpClientFactory)
            : base(tokenService, httpClientFactory)
        {
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

        [Authorize]
        public IActionResult LoginSuccess()
        {
            ViewData["Username"] = HttpContext.Session.GetString("Username");
            ViewData["Role"] = HttpContext.Session.GetString("Role");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Para login no necesitamos token autorizado
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
                    Console.WriteLine($"=== LOGIN EXITOSO ===");
                    Console.WriteLine($"Token recibido: {tokenResponse.AccessToken.Substring(0, Math.Min(50, tokenResponse.AccessToken.Length))}...");
                    Console.WriteLine($"Username: {tokenResponse.Username}");
                    Console.WriteLine($"Role: {tokenResponse.Role}");
                    
                    await _tokenService.StoreToken(HttpContext, tokenResponse.AccessToken, tokenResponse.Username ?? "", tokenResponse.Role ?? "");
                    
                    // Verificar que se guardó correctamente
                    var storedToken = _tokenService.GetToken(HttpContext);
                    Console.WriteLine($"Token almacenado verificado: {(storedToken != null ? "✅ OK" : "❌ ERROR")}");
                    
                    ViewData["Username"] = tokenResponse.Username;
                    ViewData["Role"] = tokenResponse.Role;
                    return View("LoginSuccess"); 
                }
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await _tokenService.ClearToken(HttpContext);
            return RedirectToAction("Login", "Account");
        }

        [Authorize]
        public async Task<IActionResult> Secret()
        {
            var token = _tokenService.GetToken(HttpContext);
            if (!_tokenService.IsTokenValid(HttpContext))
            {
                return RedirectToAction("Login", "Account");
            }

            var client = CreateAuthorizedClient("JWTAuthAPI");
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