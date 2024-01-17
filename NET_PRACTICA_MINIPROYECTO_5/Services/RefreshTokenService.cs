using NET_PRACTICA_MINIPROYECTO_5.Models;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;

namespace NET_PRACTICA_MINIPROYECTO_5.Services
{
    public interface IRefreshTokenService
    {
        RefreshToken GenerteRefreshToken(int idUser);
    }

    public class RefreshTokenService: IRefreshTokenService
    {
        private readonly IHttpContextAccessor  _context;
        private readonly SqlConnection _connection;


        public RefreshTokenService(IHttpContextAccessor httpContextAccessor, IDbConnectionFactory dbConnectionFactory)
        {
            _context = httpContextAccessor;
            _connection = dbConnectionFactory.CreateConnection();
        }


        public RefreshToken GenerteRefreshToken(int idUser)
        {
            RefreshToken refreshToken = new() { 
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Expires = DateTime.Now.AddDays(1)
            };
            
            _connection.Open();
            SetRefreshToken(refreshToken, SetRefreshTokenToDataBase(refreshToken), idUser);
            _connection.Close();

            return refreshToken;
        }

        public void SetRefreshToken(RefreshToken newRefreshToken, int idToken, int idUser)
        {
            CookieOptions cookieOptions = new()
            {
                HttpOnly = true,
                SameSite = SameSiteMode.None,
                Secure = true,
                Expires = newRefreshToken.Expires,
            };

            LinkTokenToUser_DataBase(idToken, idUser);

            _context.HttpContext?.Response.Cookies.Append("refreshToken", newRefreshToken.Token, cookieOptions);
        }

        public int SetRefreshTokenToDataBase(RefreshToken refreshToken)
        {
            string Query = "DECLARE @idToken INT; " +
                "INSERT INTO Proyecto_1.dbo.refreshToken (Token, Creation, Expiration) " +
                "VALUES(@Token, @Creation, @Expiration); " +
                "SET @idToken = SCOPE_IDENTITY(); " +
                "SELECT @idToken AS idToken;";

            //int idToken = -1;

            SqlCommand sqlCommand = new(Query, _connection);

            try
            {
                sqlCommand.Parameters.AddWithValue("Token", refreshToken.Token);
                sqlCommand.Parameters.AddWithValue("Creation", DateTime.Now);
                sqlCommand.Parameters.AddWithValue("Expiration", refreshToken.Expires);

                SqlDataReader reader = sqlCommand.ExecuteReader();

                while(reader.Read())
                {
                    int idToken = reader.GetInt32("idToken");
                    return idToken;
                }

                reader.Close();

                return -1;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

        public void LinkTokenToUser_DataBase(int idToken, int IdUser)
        {
            string Query = "UPDATE Proyecto_1.dbo.users " +
                "SET idToken = @idToken " +
                "WHERE users.idUser = @idUser";

            SqlCommand sqlCommand = new(Query, _connection);

            try
            {
                sqlCommand.Parameters.AddWithValue("idToken", idToken);
                sqlCommand.Parameters.AddWithValue("idUser", IdUser);

                sqlCommand.ExecuteNonQuery();

            }
            catch(Exception ex)
            {
                throw new Exception($"LinkTokenToUser_DataBase method failed: {ex.Message}");

            }
        }

    }
}
