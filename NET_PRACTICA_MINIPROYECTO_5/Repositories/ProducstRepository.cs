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

        public List<ProductWriting> GetAllbyFilter(string CuantityFilter, string CategoryFilter)
        {

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
                "All" => "",
                _ => throw new Exception(),
            };

            string Query = "SELECT products.*, " +
                "categories.Name AS Category, " +
                "users.Name AS Author " +
                "FROM products " +
                "LEFT JOIN categories ON products.idCategory = categories.idCategory " +
                "LEFT JOIN users ON products.idUser = users.idUser " +
            FilterCategory +
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

                        ImageRute = Reader.GetString("ImageBlobRute"),

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

            string Query = "INSERT INTO Proyecto_1.dbo.products(Title, Description, Stock, Price, Date, idUser, idCategory, ImageBlobRute) " +
              "VALUES(@Title, @Description, @Stock, @Price, @Date, @IdUser, @IdCategory, @ImageBlobRute)";

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
                Command.Parameters.AddWithValue("ImageBlobRute", Uri);


                Command.ExecuteNonQuery();
                _connection.Close();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void Update(ProductReading products, string Uri)
        {
            string Query = "UPDATE Proyecto_1.dbo.products " +
                "SET Title = @Title, " +
                "Description = @Description, " +
                "Stock = @Stock, " +
                "Price = @Price, " +
                "idCategory = @IdCategory, " +
                "ImageBlobRute = @ImageBlobRute " +
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
                Command.Parameters.AddWithValue("ImageBlobRute", Uri);

                Command.ExecuteNonQuery();
                _connection.Close();
            }
            catch(Exception ex) 
            {
                throw new Exception(ex.Message);
            }

        }

        public void Delete(ProductReading products)
        {
            throw new NotImplementedException();
        }


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
            catch ( Exception ex )
            {
                throw new Exception (ex.Message);
            }

        }
    }
}
