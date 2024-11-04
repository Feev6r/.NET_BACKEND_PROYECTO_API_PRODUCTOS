using NET_PRACTICA_MINIPROYECTO_5.Interfaces;
using NET_PRACTICA_MINIPROYECTO_5.Models;
using NET_PRACTICA_MINIPROYECTO_5.Services;
using System.Data;
using System.Data.SqlClient;

namespace NET_PRACTICA_MINIPROYECTO_5.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly SqlConnection _connection;

        public AuthRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _connection = dbConnectionFactory.CreateConnection();
        }

        public DateTime GetRefreshTokenExpiration(int IdUser)
        {
            string Query = "SELECT Expiration FROM refreshToken WHERE idToken = (SELECT idToken FROM users WHERE idUser = @idUser)";

            DateTime RefreshTokenExpiration = new();

            try
            {
                SqlCommand sqlCommand = new(Query, _connection);
                _connection.Open();

                sqlCommand.Parameters.AddWithValue("idUser", IdUser);

                SqlDataReader reader = sqlCommand.ExecuteReader();

                while (reader.Read())
                {
                    RefreshTokenExpiration = reader.GetDateTime("Expiration");
                }


                reader.Close();
                _connection.Close();

                return RefreshTokenExpiration;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void InsertUser(User user)
        {
            string consulta = "INSERT INTO Proyecto_1.dbo.users(Name, Email, Password) " +
              "VALUES(@Nombre, @Email, @Password)";

            try
            {
                SqlCommand sqlCommand = new(consulta, _connection);

                _connection.Open();

                sqlCommand.Parameters.AddWithValue("Nombre", user.Name);
                sqlCommand.Parameters.AddWithValue("Email", user.Email);
                sqlCommand.Parameters.AddWithValue("Password", user.Password);

                sqlCommand.ExecuteNonQuery();

                _connection.Close();

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public int UserExist(string UserName)
        {
            string Query = """ 
                                                        
              IF EXISTS (SELECT idUser FROM Proyecto_1.dbo.users WHERE Name = @UserName)
              SELECT 1 AS Exist, idUser from Proyecto_1.dbo.users where Name = @UserName
              ELSE SELECT 0 AS Exist

             """;

            int IdUser = 0;

            try
            {
                _connection.Open();

                SqlCommand sqlCommand = new(Query, _connection);

                sqlCommand.Parameters.AddWithValue("UserName", UserName);

                SqlDataReader reader = sqlCommand.ExecuteReader();


                while (reader.Read())
                {
                    int Exist = reader.GetInt32("Exist");

                    if (Exist == 0)
                    {
                        throw new Exception("invalid user name or password");
                    }

                    IdUser = reader.GetInt32("IdUser");
                }

                _connection.Close();

                return IdUser;

            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }


        }

        public string GetPasswordFromUser(string UserName)
        {

            string Query = "SELECT Password FROM Proyecto_1.dbo.users WHERE Name = @Name";
            string? Password;

            try
            {
                _connection.Open();

                SqlCommand sqlCommand = new(Query, _connection);

                sqlCommand.Parameters.AddWithValue("Name", UserName);

                Password = (string)sqlCommand.ExecuteScalar();

                _connection.Close();

                return Password!;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void CreateAndSetRefreshToken(RefreshToken refreshToken, int IdUser)
        {
            //Consider that first we need to save the idtoken that will be replace with the new one to be able to eliminate it
            string Query = """
                DECLARE @Old_idToken INT

                SELECT @Old_idToken = idToken FROM Proyecto_1.dbo.users WHERE idUser = @IdUser



                DECLARE @id_Token TABLE (id_Token INT);

                INSERT INTO Proyecto_1.dbo.refreshToken (Token, Expiration)
                OUTPUT inserted.idToken INTO @id_Token
                VALUES (@Token, @Expiration)
                
              
                DECLARE @New_IdToken INT;
                SELECT @New_IdToken = id_Token FROM @id_Token;
                
                
                UPDATE users SET idToken = @New_IdToken WHERE idUser = @IdUser

                DELETE FROM Proyecto_1.dbo.refreshToken WHERE idToken = @Old_idToken
                """
            ;

            try
            {
                SqlCommand sqlCommand = new(Query, _connection);


                sqlCommand.Parameters.AddWithValue("Token", refreshToken.Token);
                sqlCommand.Parameters.AddWithValue("Expiration", refreshToken.Expires);
                sqlCommand.Parameters.AddWithValue("IdUser", IdUser);

                _connection.Open();
                sqlCommand.ExecuteNonQuery();
                _connection.Close();


            }
            catch (Exception ex)
            {
                throw new Exception (ex.Message);
            }
        }

    }
}
