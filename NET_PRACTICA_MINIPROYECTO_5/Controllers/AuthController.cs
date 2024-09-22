using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NET_PRACTICA_MINIPROYECTO_5.Interfaces;
using NET_PRACTICA_MINIPROYECTO_5.Models;

namespace NET_PRACTICA_MINIPROYECTO_5.Controllers
{
    [Route("Auth")]
    [ApiController]
    public class AuthController : Controller
    {

        private readonly IConfiguration _configuration;
        private readonly IAntiforgery _antiforgery;
        private readonly IAuthService _authTokenService;


        public AuthController(IConfiguration configuration,
            IAuthService authTokenService,
            IAntiforgery antiforgery
            )
        {
            _configuration = configuration;
            _authTokenService = authTokenService;
            _antiforgery = antiforgery;
        }


        [HttpPost]
        [Route("signUp")]
        public ActionResult Registrar(User user)
        {

            try
            {
                _authTokenService.RegisterUser(user);

                return Ok();

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [HttpPost]
        [Route("login")]
        public ActionResult Login(User userLogin)
        {
            try
            {
                string jwtToken = _authTokenService.LoginUser(userLogin, HttpContext);

                HttpContext.Response.Cookies.Append("JWT-TOKEN", jwtToken, new CookieOptions
                {
                    //Expires = DateTime.Now.AddDays(_configuration.GetValue<double>("JwtTokenOptions:ExpirationDays")),
                    HttpOnly = true,
                    Path = "/",
                    Secure = true,
                    SameSite = SameSiteMode.None
                });
                HttpContext.Response.Cookies.Append("UserSession", "1", new CookieOptions
                {
                    HttpOnly = false,
                    Path = "/",
                    Secure = true,
                    SameSite = SameSiteMode.None
                });

                var csrfToken = _antiforgery.GetAndStoreTokens(HttpContext).RequestToken;
                HttpContext.Response.Headers.Append("X-CSRF-TOKEN", csrfToken);

                return Ok();

            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Failed to login {ex.Message}" });
            }
        }

        [HttpGet, Authorize]
        [Route("tokenCsrf")]
        public ActionResult GetCsrfToken()
        {
            try
            {
                var csrfToken = _antiforgery.GetAndStoreTokens(HttpContext).RequestToken;
                HttpContext.Response.Headers.Append("X-CSRF-TOKEN", csrfToken);
                return Ok();

            }
            catch(Exception ex) 
            {
                return BadRequest(new { message = $"Failed to login-CsrfToken {ex.Message}" });
            }

        }


        [HttpPost, Authorize]
        [Route("logout")]
        public ActionResult Logout()
        {
            try
            {
                Response.Cookies.Append("JWT-TOKEN", "",new CookieOptions { Expires = DateTime.Now.AddDays(-1), Secure = true, SameSite = SameSiteMode.None});
                Response.Cookies.Append("refreshToken", "", new CookieOptions { Expires = DateTime.Now.AddDays(-1), Secure = true, SameSite = SameSiteMode.None });
                Response.Cookies.Append(".AspNetCore.Antiforgery.bKA7B3_H_Uo", "", new CookieOptions { Expires = DateTime.Now.AddDays(-1) , Secure = true, SameSite = SameSiteMode.None });
            
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new {message = $"Error at logout {ex.Message}"});
            }

        }
    }
}

//IActionResult es una interface que te deja crear clases que la tengan de interface y puedes devolver un objeto de esa clase
//creada por ti

//ActionResult es una clase abstracta que solo te deja devolver metodos definos por ella.