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
            try
            {

                if (context.Session.GetString("CSRF_TOKEN") != null && context.Session.IsAvailable)
                {
                    await _next(context);
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    context.Response.ContentType = "text/plain";
                    await context.Response.WriteAsync($"Ocurrio un error con la session.");
                    return;
                }
            }
            catch (Exception ex)
            {
               await context.Response.WriteAsync($"Error en session: {ex.Message}");
            }


       
            //NOTA: El isAvailable solo comprueba si hay perdida con la base datos o esta mal configurada la session, no es necesario
            //en este caso por es lo que hay, y un catch por si las moscas (QUITAR ANTES DE SUBIR EL PROYECTO), este middleware es muy inecesario

        }
    }
}
