using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.Json;

namespace NET_PRACTICA_MINIPROYECTO_5.Attributes
{
    public class TokenCsrfGeneration : Attribute, IActionFilter
    {

        public TokenCsrfGeneration()
        {
        }

        //Token CSRF - Validation
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var routeData = context.HttpContext.GetRouteData();
            var controller = routeData.Values["controller"]?.ToString();
            var action = routeData.Values["action"]?.ToString();


            //When it comes to endpoint "Auth/login" the verification wont be done
            if (controller != "Auth" && action != "login")
            {
                var HeaderCsrfToken = context.HttpContext.Request.Headers["X-Csrf-Token"];

                string SessionCsrfToken = context.HttpContext.Session!.GetString("CSRF-Token")!;

                if (!Equals(HeaderCsrfToken, SessionCsrfToken))
                {
                    context.Result = new ContentResult
                    {

                        ContentType = "application/json",
                        Content = JsonSerializer.Serialize(new { message = "Mal" }),
                        StatusCode = 401
                    };
                }
            }
           
        }

        //Token CSRF - Generation
        public void OnActionExecuted(ActionExecutedContext context)
        {

            var _antiforgery = context.HttpContext.RequestServices.GetService<IAntiforgery>();

            string token = _antiforgery!.GetTokens(context.HttpContext).RequestToken!;

            context.HttpContext.Response.Headers.Append("X-CSRF-Token", token);
            context.HttpContext.Session.SetString("CSRF-Token", token);
        }

    }
}
