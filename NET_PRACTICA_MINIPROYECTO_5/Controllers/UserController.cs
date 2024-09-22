using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NET_PRACTICA_MINIPROYECTO_5.Interfaces;
using NET_PRACTICA_MINIPROYECTO_5.Models;
using System.Security.Claims;

namespace NET_PRACTICA_MINIPROYECTO_5.Controllers
{
    [Route("Information")]
    public class UserController : Controller
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [Route("user")]
        [HttpGet, Authorize]
        public ActionResult GetUserInfo()
        {
            
            ClaimsPrincipal UserClaims = HttpContext.User;

            try
            {
                User user = _userService.GetUser(UserClaims);

                return Ok(user);

            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error at GetUserInfo: {ex.Message}" });
            }

        }

    }
}
