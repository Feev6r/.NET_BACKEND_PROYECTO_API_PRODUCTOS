using NET_PRACTICA_MINIPROYECTO_5.Models;

namespace NET_PRACTICA_MINIPROYECTO_5.Interfaces
{
    public interface IAuthRepository
    {
        public DateTime GetRefreshTokenExpiration(int IdUser);

        public void InsertUser(User user);

        public string GetPasswordFromUser(string UserName);

        public void CreateAndSetRefreshToken(RefreshToken refreshToken, int IdUser);

        public int UserExist(string UserName);
    }
}
