using Microsoft.AspNetCore.Antiforgery;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

namespace NET_PRACTICA_MINIPROYECTO_5.Services
{
    public interface IAuthTokenService
    {
        public string CreateJwtToken(int IdUser);
        public string CreateCsrfToken();
        public bool IsEmailValid(string email);
        public ClaimsPrincipal? ValidateTokenJwt(string token);
        public bool ValidateExpirationRefreshToken(int idUser);
    }

    public class AuthTokenService : IAuthTokenService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly IAntiforgery _antiforgery;
        private readonly SqlConnection _connection;

        public AuthTokenService(
            IHttpContextAccessor httpContextAccessor, 
            IConfiguration configuration, 
            IAntiforgery antiforgery, 
            IDbConnectionFactory dbConnectionFactory
            
            )
        {
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _antiforgery = antiforgery;
            _connection = dbConnectionFactory.CreateConnection();
        }


        public string CreateJwtToken(int IdUser)
        {
            DateTime Expiration = DateTime.Now.AddSeconds(10);

            List<Claim> claims = new List<Claim>()
            {
                //new Claim(ClaimTypes.Name, user.Name),
                new Claim("UserId", IdUser.ToString())
            };

            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes //obtenemos los bytes de la key
                (_configuration.GetSection("AppSettings:Token").Value!));

            SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature); //la llave y el algoritmo para crear el token

            JwtSecurityToken token = new JwtSecurityToken( //issuer es quien lo emite, osea la url de nuestra api y audience es quien la recive,
                                                           //los claims son los datos que recibimos para el token, y el resto es logico
                issuer: "https://localhost:7777/",
                audience: "https://localhost:7777/Show/",
                claims: claims,
                expires: Expiration,
                signingCredentials: creds
                );

            string jwt = new JwtSecurityTokenHandler().WriteToken(token);

            _httpContextAccessor.HttpContext!.Response.Cookies.Append("JWT-TOKEN", jwt,
                new CookieOptions
                {
                    //Expires = Expiration,
                    HttpOnly = true,
                    Path = "/",
                    Secure = true,
                    SameSite = SameSiteMode.None
                });

            return jwt;
        }


        public ClaimsPrincipal? ValidateTokenJwt(string token)
        {
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

            TokenValidationParameters validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateLifetime = true,
                ValidIssuer = "https://localhost:7777/",
                ValidAudience = "https://localhost:7777/Show",
                ClockSkew = TimeSpan.Zero,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("Appsettings:Token").Value!)),
            };

            try
            {
                tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

            }
            catch (SecurityTokenExpiredException)
            {
                // Manejar el caso de token expirado
                //se devuelve el claim para obtener informacion y generar un nuevo token

                var tokenHandler2 = new JwtSecurityTokenHandler();
                var expiredToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

                ClaimsPrincipal expiredClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(expiredToken!.Claims, "jwt"));

                return expiredClaimsPrincipal;
            }
            catch (Exception ex)
            {
                // Manejar otras excepciones
                throw new Exception($"Error al validar el token: {ex.Message}");
            }

            return null;
        }

        public bool ValidateExpirationRefreshToken(int idUser)
        {
            DateTime? RefreshTokenExpiration = null;

            string Query = "SELECT Expiration FROM refreshToken WHERE idToken = (SELECT idToken FROM users WHERE idUser = @idUser)";
             _connection.Open();

            try
            {
                SqlCommand sqlCommand = new(Query, _connection);

                sqlCommand.Parameters.AddWithValue("idUser", idUser);

                SqlDataReader reader = sqlCommand.ExecuteReader();

                while (reader.Read())
                {
                    RefreshTokenExpiration = reader.GetDateTime("Expiration");
                }

                reader.Close();

                if (RefreshTokenExpiration != null && RefreshTokenExpiration > DateTime.Now)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                _httpContextAccessor.HttpContext!.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                _httpContextAccessor.HttpContext.Response.ContentType = "text/plain";
                _httpContextAccessor.HttpContext.Response.WriteAsync($"Error en ValidateExpirationRefreshToken: {ex.Message}");
                return false;
            }
            finally { _connection.Close(); }

            return false;

        }

        public string CreateCsrfToken()
        {
            AntiforgeryTokenSet tokens = _antiforgery.GetAndStoreTokens(_httpContextAccessor.HttpContext!);

            _httpContextAccessor.HttpContext!.Session.SetString("CSRF_TOKEN", tokens.RequestToken!);

            return tokens.RequestToken!;
        }


        public bool IsEmailValid(string email)
        {
            //provicional 
            string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            Regex regex = new(pattern);

            return regex.IsMatch(email);
        }

    }
}
