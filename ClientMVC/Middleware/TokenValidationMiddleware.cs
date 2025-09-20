using ClientMVC.Services;

namespace ClientMVC.Middleware
{
    public class TokenValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public TokenValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ITokenService tokenService)
        {
            // Skip validation for login page and public resources
            var path = context.Request.Path.Value?.ToLower();
            if (path == null || 
                path.Contains("/account/login") || 
                path.Contains("/account/error") ||
                path.StartsWith("/css/") || 
                path.StartsWith("/js/") || 
                path.StartsWith("/lib/"))
            {
                await _next(context);
                return;
            }

            Console.WriteLine($"TokenValidationMiddleware - Path: {path}");
            Console.WriteLine($"TokenValidationMiddleware - User authenticated: {context.User.Identity?.IsAuthenticated}");

            // If user is authenticated but token is invalid, redirect to login
            if (context.User.Identity?.IsAuthenticated == true && !tokenService.IsTokenValid(context))
            {
                Console.WriteLine("TokenValidationMiddleware - Token inválido, limpiando y redirigiendo");
                await tokenService.ClearToken(context);
                context.Response.Redirect("/Account/Login");
                return;
            }

            // If user has valid token but is not authenticated in ASP.NET Core, authenticate them
            if (context.User.Identity?.IsAuthenticated != true && tokenService.IsTokenValid(context))
            {
                Console.WriteLine("TokenValidationMiddleware - Token válido pero usuario no autenticado, reautenticando");
                var token = tokenService.GetToken(context);
                var username = context.Session.GetString("Username");
                var role = context.Session.GetString("Role");
                
                if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(role))
                {
                    await tokenService.StoreToken(context, token!, username, role);
                }
            }

            await _next(context);
        }
    }
}