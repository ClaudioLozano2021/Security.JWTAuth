
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace ProtectedApi.Controllers
{
    public class Product
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required decimal Price { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetProducts()
        {
            // Debug: Verificar token recibido
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            Console.WriteLine($"Authorization header recibido: {authHeader}");
            Console.WriteLine($"Usuario autenticado: {User.Identity?.IsAuthenticated}");
            Console.WriteLine($"Nombre usuario: {User.Identity?.Name}");
            
            if (User.Identity?.IsAuthenticated != true)
            {
                Console.WriteLine("Usuario no autenticado - devolviendo 401");
                return Unauthorized();
            }
            var products = new List<Product>
            {
                new Product { Id = 1, Name = "Laptop", Price = 1200.00m },
                new Product { Id = 2, Name = "Mouse", Price = 25.50m },
                new Product { Id = 3, Name = "Keyboard", Price = 75.00m },
                new Product { Id = 4, Name = "Monitor", Price = 300.75m },
                new Product { Id = 5, Name = "USB Hub", Price = 15.00m },
                new Product { Id = 6, Name = "Webcam", Price = 80.25m },
                new Product { Id = 7, Name = "Headphones", Price = 150.00m },
                new Product { Id = 8, Name = "Docking Station", Price = 250.00m },
                new Product { Id = 9, Name = "External Hard Drive", Price = 120.00m },
                new Product { Id = 10, Name = "Desk Lamp", Price = 40.50m }
            };

            return Ok(products);
        }

        [HttpGet("test")]
        public IActionResult TestAuth()
        {
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            Console.WriteLine($"Test endpoint - Authorization header: {authHeader}");
            
            return Ok(new { 
                Message = "Autenticación exitosa", 
                User = User.Identity?.Name,
                IsAuthenticated = User.Identity?.IsAuthenticated,
                Claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
            });
        }

        [HttpGet("health")]
        [AllowAnonymous]
        public IActionResult Health()
        {
            Console.WriteLine("Health check endpoint called");
            return Ok(new { 
                Status = "OK", 
                Timestamp = DateTime.UtcNow,
                Message = "ProtectedApi is running"
            });
        }

        [HttpGet("simple")]
        [Authorize]
        public IActionResult SimpleAuth()
        {
            Console.WriteLine("=== SimpleAuth endpoint called ===");
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            Console.WriteLine($"Authorization header: {authHeader}");
            Console.WriteLine($"User authenticated: {User.Identity?.IsAuthenticated}");
            Console.WriteLine($"User name: {User.Identity?.Name}");
            
            // Este endpoint NO pasa por el middleware de validación de sesión
            // Solo usa JWT authentication básico
            return Ok(new { 
                Message = "JWT Authentication OK",
                User = User.Identity?.Name,
                Claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList(),
                SimpleProducts = new [] { 
                    new { Id = 1, Name = "Test Product", Price = 99.99m }
                }
            });
        }
    }
}
