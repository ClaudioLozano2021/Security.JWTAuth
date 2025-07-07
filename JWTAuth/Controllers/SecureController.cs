using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JwtAuthDotNet9.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SecureController : ControllerBase
    {
        [HttpGet("test")]
        [Authorize]
        public IActionResult Test()
        {
            var username = User.Identity?.Name ?? "desconocido";
            return Ok($"Hola {username}, accediste al endpoint protegido correctamente.");
        }
    }
}
