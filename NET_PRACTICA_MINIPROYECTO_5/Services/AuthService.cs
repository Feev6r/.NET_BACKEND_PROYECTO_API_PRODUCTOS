using Microsoft.IdentityModel.Tokens;
using NET_PRACTICA_MINIPROYECTO_5.Interfaces;
using NET_PRACTICA_MINIPROYECTO_5.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NET_PRACTICA_MINIPROYECTO_5.Services
{

    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly IAuthRepository _authRepository;

        public AuthService(
            IConfiguration configuration,
            IAuthRepository authRepository

            )
        {
            _configuration = configuration;
            _authRepository = authRepository;
        }


        public string CreateJwtToken(int IdUser)
        {

            List<Claim> claims = new List<Claim>()
            {
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
                expires: /*DateTime.Now.AddDays(_configuration.GetValue<double>("JwtTokenOptions:ExpirationDays"))*/ DateTime.Now.AddSeconds(10),
                signingCredentials: creds
                );

            string jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return jwt;
        }

        public ClaimsPrincipal? ValidateTokenJwt(string token)
        {
            JwtSecurityTokenHandler tokenHandler = new();

            TokenValidationParameters validationParameters = new()
            {
                ValidateIssuerSigningKey = true,
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateLifetime = true,
                ValidIssuer = "https://localhost:7777/",
                ValidAudience = "https://localhost:7777/Show/",
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
            DateTime RefreshTokenExpiration = _authRepository.GetRefreshTokenExpiration(idUser);

            try
            {
                if (RefreshTokenExpiration > DateTime.Now)
                {
                    return true;
                }
                
                return false;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

        public void GenerteRefreshToken(RefreshToken refreshToken, int IdUser)
        {
            _authRepository.CreateAndSetRefreshToken(refreshToken, IdUser);
        }

        public void RegisterUser(User user)
        {
            try
            {
                //Hash the password before we insert the data   
                user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

                _authRepository.InsertUser(user);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

        public string LoginUser(User user, RefreshToken refreshToken)
        {
            try
            {
                //Validate if the user exist and we return the id as well
                user.Id = _authRepository.UserExist(user.Name);

                if (BCrypt.Net.BCrypt.Verify(user.Password, _authRepository.GetPasswordFromUser(user.Name)))
                {
                    string JwtToken = CreateJwtToken(user.Id);

                    GenerteRefreshToken(refreshToken, user.Id);

                    return JwtToken;
                }

                throw new Exception("Invalid user name or password");

            }
            catch(Exception ex) 
            {
                throw new Exception(ex.Message);
            }

        }

    }
}
