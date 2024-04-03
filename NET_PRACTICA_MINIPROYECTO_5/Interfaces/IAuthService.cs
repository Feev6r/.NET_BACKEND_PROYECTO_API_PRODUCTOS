using NET_PRACTICA_MINIPROYECTO_5.Models;
using System.Security.Claims;

namespace NET_PRACTICA_MINIPROYECTO_5.Interfaces
{
    public interface IAuthService
    {
        public string CreateJwtToken(int IdUser);
        public ClaimsPrincipal? ValidateTokenJwt(string token);
        public bool ValidateExpirationRefreshToken(int idUser);
        public void GenerteRefreshToken(RefreshToken refreshToken, int IdUser);
        public void RegisterUser(User user);
        public string LoginUser(User user, RefreshToken refreshToken);
    }
}
