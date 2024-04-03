using NET_PRACTICA_MINIPROYECTO_5.Interfaces;
using NET_PRACTICA_MINIPROYECTO_5.Models;
using NET_PRACTICA_MINIPROYECTO_5.Services;
using System.Data;
using System.Data.SqlClient;

namespace NET_PRACTICA_MINIPROYECTO_5.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly SqlConnection _connection;

        public UserRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _connection = dbConnectionFactory.CreateConnection();
        }

        public User GetAll(int IdUser)
        {

            string Query = "SELECT * From dbo.users WHERE idUser = @idUser";

            User user = new();

            try
            {
                _connection.Open();

                SqlCommand sqlCommand = new(Query, _connection);

                sqlCommand.Parameters.AddWithValue("idUser", IdUser);

                SqlDataReader reader = sqlCommand.ExecuteReader();

                while (reader.Read())
                {
                    user.Id = reader.GetInt32("idUser");
                    user.Name = reader.GetString("Name");
                    user.Email = reader.GetString("Email");
                }

                reader.Close();

                _connection.Close();

                return user;

            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }

        }
    }
}
