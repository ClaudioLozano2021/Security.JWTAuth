using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ClientMVC.Services;

namespace ClientMVC.Attributes
{
    public class AutoTokenValidationAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var tokenService = context.HttpContext.RequestServices.GetService<ITokenService>();
            
            if (tokenService != null)
            {
                Console.WriteLine("AutoTokenValidationAttribute - Verificando token...");
                
                // Si el usuario debe estar autenticado pero no tiene token válido
                if (context.HttpContext.User.Identity?.IsAuthenticated == true && 
                    !tokenService.IsTokenValid(context.HttpContext))
                {
                    Console.WriteLine("AutoTokenValidationAttribute - Token inválido, redirigiendo");
                    context.Result = new RedirectToActionResult("TokenExpired", "Error", null);
                    return;
                }
                
                // Si tiene token válido pero no está autenticado en ASP.NET Core
                if (context.HttpContext.User.Identity?.IsAuthenticated != true && 
                    tokenService.IsTokenValid(context.HttpContext))
                {
                    Console.WriteLine("AutoTokenValidationAttribute - Token válido, reautenticando");
                    var token = tokenService.GetToken(context.HttpContext);
                    var username = context.HttpContext.Session.GetString("Username");
                    var role = context.HttpContext.Session.GetString("Role");
                    
                    if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(role))
                    {
                        // Reautenticar automáticamente
                        _ = Task.Run(async () => await tokenService.StoreToken(context.HttpContext, token!, username, role));
                    }
                }
            }
            
            base.OnActionExecuting(context);
        }
    }
}