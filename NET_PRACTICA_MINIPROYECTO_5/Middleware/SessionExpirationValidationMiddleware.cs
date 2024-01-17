using System.Net;

namespace NET_PRACTICA_MINIPROYECTO_5.Middleware
{
    public class SessionExpirationValidationMiddleware
    {
        private readonly RequestDelegate _next;


        public SessionExpirationValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {

            if (context.Session != null && context.Session.IsAvailable)
            {

                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                context.Response.ContentType = "text/plain";
                await context.Response.WriteAsync($"Ocurrio un error con la session.");
                return;

            }

            //FALTA: controlar cuando una cookie es eliminada en el cliente

            await _next(context);
        }
    }
}
