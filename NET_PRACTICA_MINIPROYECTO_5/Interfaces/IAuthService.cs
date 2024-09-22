using NET_PRACTICA_MINIPROYECTO_5.Models;

namespace NET_PRACTICA_MINIPROYECTO_5.Interfaces
{
    public interface IAuthService
    {
        public string CreateJwtToken(int IdUser, HttpContext context);
        public bool ValidateExpirationRefreshToken(int idUser);
        public void GenerteRefreshToken(RefreshToken refreshToken, int IdUser);
        public void RegisterUser(User user);
        public string LoginUser(User user, HttpContext context);
    }
}
