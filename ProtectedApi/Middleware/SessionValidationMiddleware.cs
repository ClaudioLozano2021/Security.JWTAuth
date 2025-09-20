using ProtectedApi.Services;
using System.Security.Claims;

namespace ProtectedApi.Middleware
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
            Console.WriteLine($"SessionValidationMiddleware - Path: {context.Request.Path}");
            
            // Solo validar en endpoints que requieren autenticación
            var endpoint = context.GetEndpoint();
            var requiresAuth = endpoint?.Metadata?.GetMetadata<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>() != null;

            Console.WriteLine($"SessionValidationMiddleware - RequiresAuth: {requiresAuth}");
            Console.WriteLine($"SessionValidationMiddleware - IsAuthenticated: {context.User.Identity?.IsAuthenticated}");

            if (requiresAuth && context.User.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
                var sessionIdClaim = context.User.FindFirst("SessionId");

                Console.WriteLine($"SessionValidationMiddleware - UserId: {userIdClaim?.Value}");
                Console.WriteLine($"SessionValidationMiddleware - SessionId: {sessionIdClaim?.Value}");

                if (userIdClaim != null && sessionIdClaim != null)
                {
                    var userId = Guid.Parse(userIdClaim.Value);
                    var sessionId = sessionIdClaim.Value;

                    var isValidSession = await authService.IsSessionValidAsync(userId, sessionId);
                    Console.WriteLine($"SessionValidationMiddleware - IsValidSession: {isValidSession}");
                    
                    if (!isValidSession)
                    {
                        Console.WriteLine("SessionValidationMiddleware - Sesión inválida, devolviendo 401");
                        context.Response.StatusCode = 401;
                        await context.Response.WriteAsync("Session invalidated. Please login again.");
                        return;
                    }
                }
                else
                {
                    Console.WriteLine("SessionValidationMiddleware - Claims faltantes, devolviendo 401");
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Missing user or session claims.");
                    return;
                }
            }

            await _next(context);
        }
    }
}