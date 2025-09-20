using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;
using ClientMVC.Models;
using ClientMVC.Services;
using Microsoft.AspNetCore.Authorization;

namespace ClientMVC.Controllers
{
    public class ProductsController : BaseController
    {
        public ProductsController(ITokenService tokenService, IHttpClientFactory httpClientFactory) 
            : base(tokenService, httpClientFactory)
        {
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            Console.WriteLine("=== INICIO ProductsController.Index ===");
            
            try
            {
                var client = CreateAuthorizedClient("ProtectedAPIClient");
                Console.WriteLine($"Cliente creado para: {client.BaseAddress}");

                Console.WriteLine("Enviando request a api/products...");
                var response = await client.GetAsync("api/products");
                
                Console.WriteLine($"Response recibida - Status: {response.StatusCode}");
                Console.WriteLine($"Response - IsSuccessStatusCode: {response.IsSuccessStatusCode}");
                Console.WriteLine($"Response - ReasonPhrase: {response.ReasonPhrase}");

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("✅ Response exitosa, procesando contenido...");
                    var content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Contenido recibido: {content.Substring(0, Math.Min(200, content.Length))}...");
                    
                    var products = JsonSerializer.Deserialize<List<Product>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    Console.WriteLine($"✅ {products?.Count ?? 0} productos deserializados exitosamente");
                    return View(products);
                }
                else
                {
                    Console.WriteLine($"❌ Response no exitosa - Status: {response.StatusCode}");
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"❌ Error content: {errorContent}");
                    
                    ViewData["ErrorMessage"] = $"Error al obtener productos: {response.StatusCode} - {errorContent}";
                    
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        Console.WriteLine("❌ Error 401 - Redirigiendo a página de no autorizado");
                        return RedirectToAction("Unauthorized", "Error");
                    }
                    
                    return View(new List<Product>());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ EXCEPCIÓN en ProductsController.Index: {ex.Message}");
                Console.WriteLine($"❌ StackTrace: {ex.StackTrace}");
                
                ViewData["ErrorMessage"] = $"Error de conexión: {ex.Message}";
                return View(new List<Product>());
            }
        }


        [Authorize]
        public async Task<IActionResult> TestAuth()
        {
            var token = _tokenService.GetToken(HttpContext);
            Console.WriteLine($"Test - Token obtenido: {token?.Substring(0, Math.Min(50, token?.Length ?? 0))}...");
            
            var client = CreateAuthorizedClient("ProtectedAPIClient");
            Console.WriteLine($"Test - Authorization header: {client.DefaultRequestHeaders.Authorization?.ToString()}");

            try
            {
                var response = await client.GetAsync("api/products/test");
                var content = await response.Content.ReadAsStringAsync();
                
                ViewData["TestResult"] = $"Status: {response.StatusCode}, Content: {content}";
                ViewData["Success"] = response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                ViewData["TestResult"] = $"Error: {ex.Message}";
                ViewData["Success"] = false;
            }

            return View();
        }

        [Authorize]
        public async Task<IActionResult> HealthCheck()
        {
            Console.WriteLine("=== HEALTH CHECK ===");
            
            try
            {
                var client = _httpClientFactory.CreateClient("ProtectedAPIClient");
                Console.WriteLine($"Health check - Cliente creado para: {client.BaseAddress}");
                
                var response = await client.GetAsync("api/products/health");
                Console.WriteLine($"Health check - Status: {response.StatusCode}");
                
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Health check - Content: {content}");
                
                ViewData["HealthResult"] = $"Status: {response.StatusCode}, Content: {content}";
                ViewData["Success"] = response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Health check - Error: {ex.Message}");
                ViewData["HealthResult"] = $"Error: {ex.Message}";
                ViewData["Success"] = false;
            }
            
            return View();
        }

        [Authorize]
        public async Task<IActionResult> SimpleTest()
        {
            Console.WriteLine("=== SIMPLE TEST ===");
            
            try
            {
                var client = CreateAuthorizedClient("ProtectedAPIClient");
                Console.WriteLine($"Simple test - Cliente creado para: {client.BaseAddress}");
                
                var response = await client.GetAsync("api/products/simple");
                Console.WriteLine($"Simple test - Status: {response.StatusCode}");
                Console.WriteLine($"Simple test - IsSuccessStatusCode: {response.IsSuccessStatusCode}");
                
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Simple test - Content: {content}");
                
                ViewData["SimpleResult"] = $"Status: {response.StatusCode}, Content: {content}";
                ViewData["Success"] = response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Simple test - Error: {ex.Message}");
                ViewData["SimpleResult"] = $"Error: {ex.Message}";
                ViewData["Success"] = false;
            }
            
            return View();
        }
    }
}