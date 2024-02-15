using NET_PRACTICA_MINIPROYECTO_5.Services;
using System.Net;
using System.Security.Claims;

namespace NET_PRACTICA_MINIPROYECTO_5.Middleware
{
    public class JwtExpirationValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IAuthTokenService _authTokenService;
        private readonly IRefreshTokenService _refreshTokenService;


        public JwtExpirationValidationMiddleware(
            RequestDelegate next,
            IAuthTokenService authTokenService,
            IRefreshTokenService refreshTokenService
            )
        {
            _authTokenService = authTokenService;
            _refreshTokenService = refreshTokenService;
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                if (context.Request.Cookies["JWT-TOKEN"] != null)
                {
                    string jwtToken = context.Request.Cookies["JWT-TOKEN"]!;

                    ClaimsPrincipal? claimsPrincipal = _authTokenService.ValidateTokenJwt(jwtToken)!;


                    if (claimsPrincipal != null)
                    {
                        // Accede a los claims
                        int idUser = int.Parse(claimsPrincipal.FindFirst("UserId")!.Value);

                        //Validamos si el refreshToken no a expirado para generar otro token jwt
                        if (_authTokenService.ValidateExpirationRefreshToken(idUser))
                        {
                            _authTokenService.CreateJwtToken(idUser);
                            _refreshTokenService.GenerteRefreshToken(idUser);
                        }
                    }


                    await _next(context);

                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(new { message = $"Se produjo un error al validar la credenciales" });
                    return;
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
