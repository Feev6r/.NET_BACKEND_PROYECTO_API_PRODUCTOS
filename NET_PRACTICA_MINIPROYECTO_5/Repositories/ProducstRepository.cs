using NET_PRACTICA_MINIPROYECTO_5.Interfaces;
using NET_PRACTICA_MINIPROYECTO_5.Models;
using System.Data.SqlClient;
using NET_PRACTICA_MINIPROYECTO_5.Services;
using System.Data;

namespace NET_PRACTICA_MINIPROYECTO_5.Repositories
{
    public class ProducstRepository : IProductsRepository
    {

        private readonly SqlConnection _connection;

        public ProducstRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _connection = dbConnectionFactory.CreateConnection();
        }

        public List<ProductWriting> GetAllbyFilter(string CuantityFilter, string CategoryFilter, string UserFilter = "")
        {

            string FilterUser = "";
            
            if(UserFilter != "" && CategoryFilter == "All")
                FilterUser = $"WHERE users.idUser = {UserFilter} ";
            else if(UserFilter != "" && CategoryFilter != "All")
                FilterUser = $"AND users.idUser = {UserFilter} ";


            List<ProductWriting> Products = [];

            string FilterProduct = CuantityFilter switch
            {
                "All" => "NEWID();",
                "MaxPrice" => "Price DESC;",
                "MinPrice" => "Price;",
                _ => throw new Exception(),
            };

            string FilterCategory = CategoryFilter switch
            {
                "Vegetables" => "WHERE categories.idCategory = 3 ",
                "Fruits" => "WHERE categories.idCategory = 4 ",
                "Meats" => "WHERE categories.idCategory = 5 ",
                "Drinks" => "WHERE categories.idCategory = 6 ",
                "All" => "",
                _ => throw new Exception(),
            };

            string Query = "SELECT products.*, " +
                "categories.Name AS Category, " +
                "users.Name AS Author " +
                "FROM products " +
                "LEFT JOIN categories ON products.idCategory = categories.idCategory " +
                "LEFT JOIN users ON products.idUser = users.idUser " +
            FilterCategory + FilterUser +
                "ORDER BY " + FilterProduct;


            try
            {
                _connection.Open();
                SqlCommand Command = new(Query, _connection);

                SqlDataReader Reader = Command.ExecuteReader();
                while (Reader.Read())
                {

                    Products.Add(new ProductWriting
                    {
                        IdProduct = Reader.GetInt32("idProduct"),
                        Author = Reader.GetString("Author"),
                        Title = Reader.GetString("Title"),
                        Description = Reader.GetString("Description"),
                        Stock = Reader.GetInt32("Stock"),
                        Price = Reader.GetDecimal("Price"),

                        ImageRute = Reader.GetString("UrlImage"),

                        Date = Reader.GetDateTime("Date").ToString("yyyy-MM-dd"),
                        Category = Reader.GetString("Category"),
                        IdCategory = Reader.GetInt32("idCategory")

                    });
                }

                Reader.Close();
                _connection.Close();

                return Products;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

        public void CreateNew(ProductReading product_Reading, string Uri)
        {

            string Query = "INSERT INTO Proyecto_1.dbo.products(Title, Description, Stock, Price, Date, idUser, idCategory, UrlImage) " +
              "VALUES(@Title, @Description, @Stock, @Price, @Date, @IdUser, @IdCategory, @UrlImage)";

            try
            {
                _connection.Open();
                SqlCommand Command = new(Query, _connection);


                Command.Parameters.AddWithValue("Title", product_Reading.Title);
                Command.Parameters.AddWithValue("Description", product_Reading.Description);
                Command.Parameters.AddWithValue("Stock", product_Reading.Stock);
                Command.Parameters.AddWithValue("Price", product_Reading.Price);

                Command.Parameters.AddWithValue("Date", product_Reading.Date);

                Command.Parameters.AddWithValue("idUser", product_Reading.IdUser);
                Command.Parameters.AddWithValue("idCategory", product_Reading.IdCategory);
                Command.Parameters.AddWithValue("UrlImage", Uri);


                Command.ExecuteNonQuery();
                _connection.Close();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void Update(ProductReading products, string? url)
        {
            string uriQuery = url != null ? ", UrlImage = @UrlImage " : ""; 

            string Query = "UPDATE Proyecto_1.dbo.products " +
                "SET Title = @Title, " +
                "Description = @Description, " +
                "Stock = @Stock, " +
                "Price = @Price, " +
                "idCategory = @IdCategory " +
                uriQuery +
                "WHERE idProduct = @IdProduct;";

            try
            {
                _connection.Open();

                SqlCommand Command = new(Query, _connection);

                Command.Parameters.AddWithValue("Title", products.Title);
                Command.Parameters.AddWithValue("Description", products.Description);
                Command.Parameters.AddWithValue("Stock", products.Stock);
                Command.Parameters.AddWithValue("Price", products.Price);
                Command.Parameters.AddWithValue("IdCategory", products.IdCategory);
                Command.Parameters.AddWithValue("IdProduct", products.IdProduct);
                if(url != null) Command.Parameters.AddWithValue("UrlImage", url);

                Command.ExecuteNonQuery();
                _connection.Close();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

        public void Delete(int IdProduct)
        {
            string Query = """     
                DELETE FROM Proyecto_1.dbo.products WHERE idProduct = @IdProduct
             """;

            try
            {
                _connection.Open();

                var Command = new SqlCommand(Query, _connection);

                Command.Parameters.AddWithValue("IdProduct", IdProduct);

                Command.ExecuteNonQuery();

                _connection.Close();

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

        //method to getimageuri of blobs
        public string GetImageUri(int ProductId)
        {
            string Query = "SELECT ImageBlobRute FROM Proyecto_1.dbo.products " +
                "WHERE products.idProduct = @IdProduct;";

            string? Uri = null;

            try
            {
                _connection.Open();
                SqlCommand Command = new(Query, _connection);

                Command.Parameters.AddWithValue("IdProduct", ProductId);
                SqlDataReader Reader = Command.ExecuteReader();

                while (Reader.Read())
                {
                    Uri = Reader.GetString("ImageBlobRute");
                }

                Reader.Close();
                _connection.Close();

                return Uri!;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public string getImagePublicId(int ProductId)
        {
            try
            {
                string Query = "SELECT UrlImage FROM Proyecto_1.dbo.products " +
                     "WHERE products.idProduct = @IdProduct;";

                string url = "";

                _connection.Open();
                SqlCommand Command = new(Query, _connection);

                Command.Parameters.AddWithValue("IdProduct", ProductId);
                SqlDataReader Reader = Command.ExecuteReader();

                while (Reader.Read())
                {
                    url = Reader.GetString("UrlImage");
                }

                Reader.Close();
                _connection.Close();

                return url.Substring(13);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string VerifyUser(int IdUser, int IdProduct)
        {
            try
            {
                string Query = """
                IF @IdUser = (SELECT idUser FROM Proyecto_1.dbo.products WHERE idProduct = @IdProduct)
                SELECT 1 AS Match
                ELSE SELECT 0 AS Match
                """;

                _connection.Open();

                var Command = new SqlCommand(Query, _connection);

                Command.Parameters.AddWithValue("IdUser", IdUser);
                Command.Parameters.AddWithValue("IdProduct", IdProduct);


                string Match = Command.ExecuteScalar().ToString()!;

                _connection.Close();

                return Match;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }
    }
}
