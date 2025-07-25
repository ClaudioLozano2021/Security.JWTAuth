using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ProtectedApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SecretController : ControllerBase
    {
        [HttpGet]
        [Authorize]
        public IActionResult GetSecret()
        {
            return Ok("This is a secret message from the protected API.");
        }
    }
}
