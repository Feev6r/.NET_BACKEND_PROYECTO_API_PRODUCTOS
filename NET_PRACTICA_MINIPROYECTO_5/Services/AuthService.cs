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

        public string CreateJwtToken(int IdUser, HttpContext context)
        {

            List<Claim> claims = new List<Claim>()
            {
                new Claim("UserId", IdUser.ToString())
            };

            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes //obtenemos los bytes de la key
                (_configuration.GetSection("AppSettings:Token").Value!)); //entramos a configuration y tomamos el valor

            SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature); //la llave y el algoritmo para crear el token

            JwtSecurityToken token = new JwtSecurityToken( //issuer es quien lo emite, osea la url de nuestra api y audience es quien la recive,
                                                           //los claims son los datos que recibimos para el token, y el resto es logico
                issuer: "https://localhost:7777/",
                audience: "http://localhost:4200/",
                claims: claims,
                expires: DateTime.Now.AddDays(_configuration.GetValue<double>("JwtTokenOptions:ExpirationDays")),
                signingCredentials: creds
                );

            //Update the context.user to avoid possible discrepancies with csrftoken validator
            var claimsIdentity = new ClaimsIdentity(token.Claims, "Bearer");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            context.User = claimsPrincipal;

            //return the string token
            return new JwtSecurityTokenHandler().WriteToken(token); 
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
                //Hash the password before we insert the all the user data   
                user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

                _authRepository.InsertUser(user);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

        public string LoginUser(User user, HttpContext context)
        {
            try
            {
                //Validate if the user exist and return the id as well
                user.Id = _authRepository.UserExist(user.Name);

                if (BCrypt.Net.BCrypt.Verify(user.Password, _authRepository.GetPasswordFromUser(user.Name)))
                {
                    string JwtToken = CreateJwtToken(user.Id, context);

                    GenerteRefreshToken(new RefreshToken(), user.Id);

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
