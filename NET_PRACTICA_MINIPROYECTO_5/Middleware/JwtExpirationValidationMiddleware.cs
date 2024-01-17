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
            if (context.Request.Cookies["JWT-TOKEN"] != null)
            {
                string jwtToken = context.Request.Cookies["JWT-TOKEN"]!;

                ClaimsPrincipal? claimsPrincipal = _authTokenService.ValidateTokenJwt(jwtToken)!;

                try
                {
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
                    else
                    {
                        await context.Response.WriteAsync("Error al procesar las credenciales");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    await context.Response.WriteAsync($"Error at JwtExpirationValidationMiddleware: {ex.Message}");
                    return;
                }

                await _next(context);

            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                context.Response.ContentType = "text/plain";
                await context.Response.WriteAsync($"Se produjo un error al validar la credenciales");
                return;
            }
        }
    }
}
