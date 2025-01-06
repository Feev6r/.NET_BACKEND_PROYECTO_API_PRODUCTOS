using NET_PRACTICA_MINIPROYECTO_5.Models;
using System.Security.Claims;

namespace NET_PRACTICA_MINIPROYECTO_5.Interfaces
{
    public interface IUserService
    {
        public User GetUser(ClaimsPrincipal UserClaim);
    }
}
