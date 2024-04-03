using System.Security.Cryptography;

namespace NET_PRACTICA_MINIPROYECTO_5.Models
{
    public class RefreshToken
    {
        public string Token { get; }
        public DateTime Expires { get; }


        public RefreshToken()
        {
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            Expires = DateTime.Now.AddDays(1);
        }

        public CookieOptions RefreshTokenCookie()
        {
            CookieOptions cookieOptions = new()
            {
                HttpOnly = true,
                SameSite = SameSiteMode.None,
                Secure = true,
                Expires = Expires
            };

            return cookieOptions;
        }

    }
}
