using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NET_PRACTICA_MINIPROYECTO_5.Models;
using NET_PRACTICA_MINIPROYECTO_5.Services;
using System.Data;
using System.Data.SqlClient;
using System.Text.Json;

namespace NET_PRACTICA_MINIPROYECTO_5.Controllers
{
    [Route("Auth")] 
    [ApiController]
    public class AuthController : Controller
    {

        private readonly SqlConnection _connection;
        private readonly IAntiforgery _antiforgery;
        private readonly IConfiguration _configuration1;
        private readonly IAuthTokenService _authTokenService;
        private readonly IRefreshTokenService _refreshTokenService;


        public AuthController(IConfiguration configuration, 
            IDbConnectionFactory dbConnectionFactory, 
            IAntiforgery antiforgery, 
            IAuthTokenService  authTokenService, 
            IRefreshTokenService refreshTokenService   
            ) 
        {
            _configuration1 = configuration;
            _antiforgery = antiforgery;
            _connection = dbConnectionFactory.CreateConnection();
            _authTokenService = authTokenService;
            _refreshTokenService = refreshTokenService;
        }


        [HttpPost]
        [Route("signUp")]
        public async Task<IActionResult> Registrar(dynamic userDto)
        {
            string consulta = "INSERT INTO Proyecto_1.dbo.users(Name, Email, Password) " +
                "VALUES(@Nombre, @Email, @Password)";


            try
            {
                await _connection.OpenAsync();

                SqlCommand sqlCommand = new(consulta, _connection);

                User user = JsonSerializer.Deserialize<User>(userDto.ToString());

                if (!_authTokenService.IsEmailValid(user.Email))
                {
                    return BadRequest("Email Invalido");
                }

                string passwordHash = BCrypt.Net.BCrypt.HashPassword(user.Password);

                sqlCommand.Parameters.AddWithValue("Nombre", user.Name);
                sqlCommand.Parameters.AddWithValue("Email", user.Email);
                sqlCommand.Parameters.AddWithValue("Password", passwordHash);

                await sqlCommand.ExecuteScalarAsync();



                return Ok("Exitoso");

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            finally
            {
                await _connection.CloseAsync();
            }

            //IActionResult es una interface que te deja crear clases que la tengan de interface y puedes devolver un objeto de esa clase
            //creada por ti

            //ActionResult es una clase abstracta que solo te deja devolver metodos definos por ella.

        }


        [HttpPost]
        [Route("login")]
        public async Task<ActionResult> Login(dynamic userLoginD)
        {
            string Query = "IF EXISTS (SELECT Name, Password FROM Proyecto_1.dbo.users  WHERE Name = @Name) " +
                "SELECT 1 AS ExisteDato, Password, idUser From users where Name = @Name " +
                "ELSE SELECT 0 AS ExisteDato ";


            //hacer deserializacion para devolver errores personalizados
            User userLogin = JsonSerializer.Deserialize<User>(userLoginD.ToString());

            User user = new User();

            try
            {
                await _connection.OpenAsync();

                SqlCommand sqlCommand = new(Query, _connection);

                sqlCommand.Parameters.AddWithValue("Name", userLogin.Name);

                await sqlCommand.ExecuteScalarAsync();

                SqlDataReader reader = await sqlCommand.ExecuteReaderAsync();


                while (await reader.ReadAsync())
                {
                    int Exist = reader.GetInt32("ExisteDato");


                    if (Exist == 0) //Verficacion Nombre
                    {
                        return BadRequest("Nombre o Contraseña incorrectos");
                    }

                    user.Password = reader.GetString("Password");
                    user.Id = reader.GetInt32("idUser");
                }

                await reader.CloseAsync();
                await _connection.CloseAsync();

                //Verificacion Constraseña
                if (!BCrypt.Net.BCrypt.Verify(userLogin.Password, user.Password))
                {
                    return BadRequest("Nombre o Contraseña incorrectos");
                }

                //Creacion Token JWT
                _authTokenService.CreateJwtToken(user.Id);
                //Refresh Token
                _refreshTokenService.GenerteRefreshToken(user.Id);
                //Token Csrf
                return Ok(_authTokenService.CreateCsrfToken());

            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to login {ex.Message}");
            }       
        }

        //en constuccion ----
        [HttpPost, Authorize]
        [Route("logout")]
        public IActionResult Logout()
        {
            try
            {
                // Crear una nueva instancia de la cookie y establecer el mismo nombre
                HttpContext.Response.Cookies.Append("JWT-TOKEN", "VOID",
                        new CookieOptions
                        {
                            Expires = DateTime.Now.AddDays(-1),
                            HttpOnly = true,
                            Path = "Show/Productos",
                            Secure = true,
                            SameSite = SameSiteMode.None
                        });

                //-----FALTA----
                //eliminar todo, token scrf, session completa con cookie, refresh token cookie.

                return Ok("HECHO Y DERECHO");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
    }
}
