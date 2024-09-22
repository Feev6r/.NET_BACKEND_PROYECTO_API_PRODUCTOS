using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Tokens;
using NET_PRACTICA_MINIPROYECTO_5.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;


namespace NET_PRACTICA_MINIPROYECTO_5.Attributes
{
    public class RefreshCredentials : IAuthorizationFilter
    {
        private readonly IAuthService _authService;
        private readonly IAntiforgery _antiforgery;

        public RefreshCredentials(IAuthService authService, IAntiforgery antiforgery)
        {
            _authService = authService;
            _antiforgery = antiforgery;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            try
            {
                if (context.HttpContext.Request.Cookies["JWT-TOKEN"] != null)
                {   
                    //we get the jwtToken if the cookie exist
                    string JwtToken = context.HttpContext.Request.Cookies["JWT-TOKEN"]!;

                    JwtSecurityToken jwtSecurityToken = new(JwtToken); //convert it to JwtSecurityToken
                    var JwtTokenExpiration = jwtSecurityToken.ValidTo; //get the expiration value

                    if (JwtTokenExpiration < DateTime.UtcNow)
                    {
                        int IdUser = int.Parse(context.HttpContext.User.FindFirst("UserId")!.Value);

                        //verify in the DB if the expirationToken doesnt have expired already
                        if (_authService.ValidateExpirationRefreshToken(IdUser))
                        {
                            //Generate JwtToken - RefreshToken
                            var jwtToken = _authService.CreateJwtToken(IdUser, context.HttpContext);
                            _authService.GenerteRefreshToken(new Models.RefreshToken(), IdUser);


                            //send (JwtToken - CsrfToken) Cookies
                            context.HttpContext.Response.Cookies.Append("JWT-TOKEN", jwtToken, new CookieOptions
                            {
                                //Expires = DateTime.Now.AddDays(_configuration.GetValue<double>("JwtTokenOptions:ExpirationDays")),
                                HttpOnly = true,
                                Path = "/",
                                Secure = true,
                                SameSite = SameSiteMode.None
                            });
                            var csrfToken = _antiforgery.GetAndStoreTokens(context.HttpContext).RequestToken;
                            context.HttpContext.Response.Headers.Append("X-CSRF-TOKEN", csrfToken);
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
            catch (SecurityTokenException)
            {
                context.Result = new ContentResult
                {
                    ContentType = "application/json",
                    Content = JsonSerializer.Serialize(new { message = "TokenJWT error" }),
                    StatusCode = 401
                };
            }
            catch (Exception)
            {
                context.Result = new ContentResult
                {
                    ContentType = "application/json",
                    Content = JsonSerializer.Serialize(new { message = "TokenJWT validation error" }),
                    StatusCode = 401
                };
            }

        }

    }

}

