using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace ClientMVC.Services
{
    public interface ITokenService
    {
        Task StoreToken(HttpContext context, string token, string username, string role);
        string? GetToken(HttpContext context);
        bool IsTokenValid(HttpContext context);
        Task ClearToken(HttpContext context);
    }

    public class TokenService : ITokenService
    {
        public async Task StoreToken(HttpContext context, string token, string username, string role)
        {
            // Store in session
            context.Session.SetString("JWToken", token);
            context.Session.SetString("Username", username);
            context.Session.SetString("Role", role);

            // Also store in cookie-based authentication for authorization attributes
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role),
                new Claim("Token", token)
            };

            var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
            var authProperties = new Microsoft.AspNetCore.Authentication.AuthenticationProperties
            {
                IsPersistent = false,
                ExpiresUtc = DateTime.UtcNow.AddMinutes(30)
            };

            await context.SignInAsync("Cookies", new ClaimsPrincipal(claimsIdentity), authProperties);
        }

        public string? GetToken(HttpContext context)
        {
            // Try to get from session first
            var token = context.Session.GetString("JWToken");
            if (!string.IsNullOrEmpty(token))
            {
                Console.WriteLine($"Token recuperado de sesión: {token.Substring(0, Math.Min(50, token.Length))}...");
                return token;
            }

            // Fallback to claims if session is lost
            var claimToken = context.User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(claimToken))
            {
                Console.WriteLine($"Token recuperado de claims: {claimToken.Substring(0, Math.Min(50, claimToken.Length))}...");
                return claimToken;
            }
            
            Console.WriteLine("No se encontró token en sesión ni en claims");
            return null;
        }

        public bool IsTokenValid(HttpContext context)
        {
            var token = GetToken(context);
            var isValid = !string.IsNullOrEmpty(token);
            Console.WriteLine($"Token válido: {isValid}");
            return isValid;
        }

        public async Task ClearToken(HttpContext context)
        {
            context.Session.Clear();
            await context.SignOutAsync("Cookies");
        }
    }
}