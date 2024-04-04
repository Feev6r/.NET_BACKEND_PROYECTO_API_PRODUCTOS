using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NET_PRACTICA_MINIPROYECTO_5.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;


namespace NET_PRACTICA_MINIPROYECTO_5.Attributes
{
    public class TestAuthorizationFilter : IAuthorizationFilter
    {
        private readonly IAuthService _authService;

        public TestAuthorizationFilter(IAuthService authService)
        {
            _authService = authService;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {

            if (context.HttpContext.Request.Cookies["JWT-TOKEN"] != null)
            {
                string JwtToken = context.HttpContext.Request.Cookies["JWT-TOKEN"]!;

                JwtSecurityToken jwtSecurityToken = new(JwtToken);
  
                var JwtTokenExpiration = jwtSecurityToken.ValidTo;



                if (JwtTokenExpiration < DateTime.UtcNow)
                {

                    int IdUser = int.Parse(context.HttpContext.User.FindFirst("UserId")!.Value);

                    if (IdUser != 0)
                    {
                        if (_authService.ValidateExpirationRefreshToken(IdUser))
                        {
                            _authService.CreateJwtToken(IdUser);
                            _authService.GenerteRefreshToken(new Models.RefreshToken(), IdUser);
                        }
                        else
                        {
                            context.Result = new ContentResult
                            {

                                ContentType = "application/json",
                                Content = JsonSerializer.Serialize(new { message = "Expired Token" }),
                                StatusCode = 401
                            };
                        }
                    }


                }

            }

        }

    }
}
