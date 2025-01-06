using System.Data.SqlClient;

namespace NET_PRACTICA_MINIPROYECTO_5.Services
{
    public interface IDbConnectionFactory 
    {
        SqlConnection CreateConnection();
    }

    public class DbConnectionFactory: IDbConnectionFactory
    {
        private readonly string _sqlConnection;

        public DbConnectionFactory(string sqlConnection) 
        {
            _sqlConnection = sqlConnection;
        }
    
        public SqlConnection CreateConnection()
        {
            return new SqlConnection(_sqlConnection);
        }
        
    }
}
