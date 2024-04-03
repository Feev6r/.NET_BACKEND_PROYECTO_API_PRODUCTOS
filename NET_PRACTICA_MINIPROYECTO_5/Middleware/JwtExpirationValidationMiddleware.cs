using NET_PRACTICA_MINIPROYECTO_5.Interfaces;
using System.Net;
using System.Security.Claims;

namespace NET_PRACTICA_MINIPROYECTO_5.Middleware
{
    public class JwtExpirationValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtExpirationValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, IAuthService authService)
        {

            try
            {
                var routeData = context.GetRouteData();
                var controller = routeData.Values["controller"]?.ToString();
                var action = routeData.Values["action"]?.ToString();

                if (controller == "Products" && action == "Test")
                {
          
                    if (context.Request.Cookies["JWT-TOKEN"] != null)
                    {
                        string jwtToken = context.Request.Cookies["JWT-TOKEN"]!;   

                        ClaimsPrincipal? claimsPrincipal = authService.ValidateTokenJwt(jwtToken)!;


                        if (claimsPrincipal != null)
                        {
                            //Access to the claims
                            int idUser = int.Parse(claimsPrincipal.FindFirst("UserId")!.Value);

                            //we validate the refresh token expiration in order to generate another jwt token
                            if (authService.ValidateExpirationRefreshToken(idUser))
                            {
                                authService.CreateJwtToken(idUser);
                                authService.GenerteRefreshToken(new Models.RefreshToken(), idUser);
                            }
                        }


                        await _next(context);

                    }
                    else //There's no cookie D:
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsJsonAsync(new { message = $"Se produjo un error al validar la credenciales" });
                        return;
                    }
                }
                else
                {
                    await _next(context);

                }



            }
            catch (Exception ex)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new { message = $"Error at JwtExpirationValidationMiddleware: {ex.Message}" });
                return;
            }



        }
    }
}
