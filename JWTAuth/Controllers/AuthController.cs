using JwtAuthDotNet9.Entities;
using JwtAuthDotNet9.Models;
using JwtAuthDotNet9.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JwtAuthDotNet9.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(UserDto request)
        {
            var user = await authService.RegisterAsync(request);
            if (user is null)
                return BadRequest("Username already exists.");

            return Ok(user);
        }

        [HttpPost("login")]
        public async Task<ActionResult<TokenResponseDto>> Login(UserDto request)
        {
            var clientIp = GetClientIpAddress();
            var result = await authService.LoginAsync(request, clientIp);
            if (result is null)
                return BadRequest("Invalid username or password.");

            return Ok(result);
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<TokenResponseDto>> RefreshToken(RefreshTokenRequestDto request)
        {
            var result = await authService.RefreshTokensAsync(request);
            if (result is null || result.AccessToken is null || result.RefreshToken is null)
                return Unauthorized("Invalid refresh token.");

            return Ok(result);
        }

        [Authorize]
        [HttpGet]
        public IActionResult AuthenticatedOnlyEndpoint()
        {
            return Ok("You are authenticated!");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin-only")]
        public IActionResult AdminOnlyEndpoint()
        {
            return Ok("You are and admin!");
        }

        [Authorize]
        [HttpGet("session-info")]
        public IActionResult GetSessionInfo()
        {
            var username = User.Identity?.Name;
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var sessionId = User.FindFirst("SessionId")?.Value;
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            return Ok(new
            {
                Username = username,
                UserId = userId,
                SessionId = sessionId,
                Role = role,
                ClientIp = GetClientIpAddress()
            });
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim != null)
            {
                var userId = Guid.Parse(userIdClaim.Value);
                await authService.LogoutAsync(userId);
            }

            return Ok("Logged out successfully");
        }

        private string? GetClientIpAddress()
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;
            if (ipAddress is not null)
            {
                // Si es una IP local de IPv6, convertir a IPv4
                if (ipAddress.IsIPv4MappedToIPv6)
                {
                    return ipAddress.MapToIPv4().ToString();
                }
                return ipAddress.ToString();
            }
            return null;
        }
    }
}
