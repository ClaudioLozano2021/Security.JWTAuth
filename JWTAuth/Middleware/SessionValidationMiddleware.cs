using JwtAuthDotNet9.Services;
using System.Security.Claims;

namespace JwtAuthDotNet9.Middleware
{
    public class SessionValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public SessionValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IAuthService authService)
        {
            // Solo validar en endpoints que requieren autenticación
            var endpoint = context.GetEndpoint();
            var requiresAuth = endpoint?.Metadata?.GetMetadata<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>() != null;

            if (requiresAuth && context.User.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
                var sessionIdClaim = context.User.FindFirst("SessionId");

                if (userIdClaim != null && sessionIdClaim != null)
                {
                    var userId = Guid.Parse(userIdClaim.Value);
                    var sessionId = sessionIdClaim.Value;

                    var isValidSession = await authService.IsSessionValidAsync(userId, sessionId);
                    
                    if (!isValidSession)
                    {
                        context.Response.StatusCode = 401;
                        await context.Response.WriteAsync("Session invalidated. Please login again.");
                        return;
                    }
                }
            }

            await _next(context);
        }
    }
}