using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace NET_PRACTICA_MINIPROYECTO_5.Attributes
{
    public class ValidateTokensCsrf : Attribute, IActionFilter
    {

        public ValidateTokensCsrf()
        {

        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
/*           var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<ValidateTokensCsrf>>();

            logger.LogInformation(context.HttpContext.Session.GetString("Expiration-JWT-TOKEN"));*/
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {

           

            string csrfToken = context.HttpContext.Session!.GetString("CSRF_TOKEN")!;

            if(csrfToken !=  null)
            {
                if (!Equals(context.HttpContext.Request.Headers["X-Csrf-Token"], csrfToken))
                {
                    context.Result = new UnauthorizedObjectResult(new { message = "Credenciales incorrectas." });
                    

                }
            }
            else
            {
                context.Result = new UnauthorizedObjectResult(new {message = "Credenciales incompletas." });
            }

            //context.Result = new BadRequestObjectResult("Algo salio mal...");
        }
    }
}
