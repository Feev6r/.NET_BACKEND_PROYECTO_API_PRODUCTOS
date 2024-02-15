using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NET_PRACTICA_MINIPROYECTO_5.Attributes;
using NET_PRACTICA_MINIPROYECTO_5.Models;
using NET_PRACTICA_MINIPROYECTO_5.Services;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Security.Claims;

namespace NET_PRACTICA_MINIPROYECTO_5.Controllers
{
    [Route("Information")]
    public class UserController : Controller
    {
        private readonly SqlConnection _connection;

        public UserController(IDbConnectionFactory dbConnectionFactory)
        {
            _connection = dbConnectionFactory.CreateConnection();
        }


        [Route("User")]
        [HttpGet, Authorize, ValidateTokensCsrf]
        public ActionResult GetUserInfo()
        {

            ClaimsPrincipal UserClaims = HttpContext.User;

            string Query = "SELECT Name From dbo.users WHERE idUser = @idUser";
            SqlCommand sqlCommand = new(Query, _connection);

            //string name = "";

            User user = new();

            try
            {
                _connection.Open();

                int idUser = int.Parse(UserClaims.FindFirst("UserId")!.Value);
                sqlCommand.Parameters.AddWithValue("idUser", idUser);

                SqlDataReader reader = sqlCommand.ExecuteReader();     

                while (reader.Read())
                {
                    user.Name = reader.GetString("Name");
                }

                reader.Close();

                return Ok(user);

            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error at GetUserInfo: {ex.Message}" });
            }
            finally { _connection.Close(); }

        }

    }
}
