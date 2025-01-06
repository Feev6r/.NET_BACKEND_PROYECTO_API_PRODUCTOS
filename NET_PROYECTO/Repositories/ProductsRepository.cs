using NET_PRACTICA_MINIPROYECTO_5.Interfaces;
using NET_PRACTICA_MINIPROYECTO_5.Models;
using System.Data.SqlClient;
using NET_PRACTICA_MINIPROYECTO_5.Services;
using System.Data;

namespace NET_PRACTICA_MINIPROYECTO_5.Repositories
{
    public class ProductsRepository : IProductsRepository
    {

        private readonly SqlConnection _connection;

        public ProductsRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _connection = dbConnectionFactory.CreateConnection();
        }

        public List<ProductWriting> GetAllByFilter(string quantityFilter, string categoryFilter, string userFilter = "", bool isOrder = false)
        {

            //Codigo claramente mejorable, desde el principio se hizo una bola de nieva, hace falta una refactorizacion.
            string filterUser = "";
            string queryOrder = "";
            string queryNumOrder = "";
            List<ProductWriting> Products = [];

            if (isOrder) {
                queryOrder = "inner JOIN orders ON products.idProduct = orders.idProduct ";
                queryNumOrder = ", orders.Quantity AS OrderQuantity, orders.idOrder AS IdOrder ";
            } 

            if (userFilter != "" && categoryFilter == "All")
                filterUser = $"WHERE users.idUser = {userFilter} ";
            else if(userFilter != "" && categoryFilter != "All")
                filterUser = $"AND users.idUser = {userFilter} ";


            string filterProduct = quantityFilter switch
            {
                "All" => "NEWID();",
                "MaxPrice" => "Price DESC;",
                "MinPrice" => "Price;",
                _ => throw new Exception(),
            };

            string filterCategory = categoryFilter switch
            {
                "Vegetables" => "WHERE categories.idCategory = 3 ",
                "Fruits" => "WHERE categories.idCategory = 4 ",
                "Meats" => "WHERE categories.idCategory = 5 ",
                "Drinks" => "WHERE categories.idCategory = 6 ",
                "All" => "",
                _ => throw new Exception(),
            };

            string Query = !isOrder ? "SELECT products.*, " +
                "categories.Name AS Category, " +
                "users.Name AS Author " +
                queryNumOrder +
                "FROM products " +
                "LEFT JOIN categories ON products.idCategory = categories.idCategory " +
                "LEFT JOIN users ON products.idUser = users.idUser " +
                queryOrder +
                filterCategory + filterUser +
                "ORDER BY " + filterProduct 
                
                :

                 "SELECT products.*, " +
                "categories.Name AS Category, " +
                "users.Name AS Author " +
                queryNumOrder +
                "FROM products " +
                "LEFT JOIN categories ON products.idCategory = categories.idCategory " +
                queryOrder +
                "LEFT JOIN users ON orders.idUser = users.idUser " +
                filterCategory + filterUser +
                "ORDER BY " + filterProduct;

            try
            {
                _connection.Open();
                SqlCommand Command = new(Query, _connection);

                SqlDataReader Reader = Command.ExecuteReader();
                while (Reader.Read())
                {

                    ProductWriting Product = new ProductWriting
                    {
                        IdProduct = Reader.GetInt32("idProduct"),
                        Author = Reader.GetString("Author"),
                        Title = Reader.GetString("Title"),
                        Description = Reader.GetString("Description"),
                        Stock = Reader.GetInt32("Stock"),
                        Price = Reader.GetDecimal("Price"),
                        ImageRoute = Reader.GetString("UrlImage"),
                        Date = Reader.GetDateTime("Date").ToString("yyyy-MM-dd"),
                        Category = Reader.GetString("Category"),
                        IdCategory = Reader.GetInt32("idCategory"),
                    };

                    if (isOrder) {
                        Product.OrderQuantity = Reader.GetInt32("OrderQuantity");
                        Product.IdOrder = Reader.GetInt32("IdOrder");
                    } 

                    Products.Add(Product);
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

        //method to get blob's image uri 
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


        public string GetImagePublicId(int ProductId)
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


        //public bool VerifyStock

        public void CreateOrder(OrderModel orderModel)
        {
            //just in case if the quantity exceeds the current stack quantity the data base will not execute anything.
            string Query = """     
             DECLARE @CurrentStock INT;

             Select @CurrentStock = Proyecto_1.dbo.products.Stock from Proyecto_1.dbo.products
             Where idProduct = @IdProduct

             IF @Quantity <= @CurrentStock
             BEGIN 
             insert into Proyecto_1.dbo.orders(Quantity, Date, idUser, idProduct)
             Values (@Quantity, @Date, @IdUser, @IdProduct)

             update Proyecto_1.dbo.products 
             set Stock -= @Quantity
             where idProduct = @IdProduct
             END

             ELSE

             BEGIN
              PRINT 'Exceeds the stack limit';
             END

             """;

            try
            {
                _connection.Open();
                var Command = new SqlCommand(Query, _connection);

                Command.Parameters.AddWithValue("Quantity", orderModel.Quantity);
                Command.Parameters.AddWithValue("Date", orderModel.Date);
                Command.Parameters.AddWithValue("IdUser",orderModel.IdUser);
                Command.Parameters.AddWithValue("IdProduct",orderModel.IdProduct);

                Command.ExecuteNonQuery();
                _connection.Close();

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public int GetTotalOrders(int idUser)
        {
            int total = 0;
            string Query = """     
             SELECT COUNT(*) AS totalOrders
             FROM Proyecto_1.dbo.orders
             WHERE orders.idUser = @idUser;
             """;

            try
            {
                _connection.Open();
                var Command = new SqlCommand(Query, _connection);
                Command.Parameters.AddWithValue("idUser", idUser);
                total = (int)Command.ExecuteScalar();
                _connection.Close();

                return total;

            }
            catch (Exception ex) { 
                throw new Exception(ex.Message);
            }
        }

        public decimal GetTotalPriceOrders(int idUser)
        {
            decimal totalPrice = 0;
            string Query = """     
             SELECT SUM(o.Quantity * p.price) AS TotalPrice
             FROM orders o
             INNER JOIN products p ON o.idProduct = p.idProduct
             WHERE o.idUser = @idUser;
             """;

            try
            {
                _connection.Open();
                var Command = new SqlCommand(Query, _connection);
                Command.Parameters.AddWithValue("idUser", idUser);
                totalPrice = (decimal)Command.ExecuteScalar();
                _connection.Close();

                return totalPrice;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void DeleteOrders(int idUser, int idOrder)
        {

            string filter = idOrder != 0 ? " and orders.idOrder = @idOrder" : "";
            string Query = "DELETE FROM orders where orders.idUser = @idUser" + filter;

            try
            {
                _connection.Open();
                var Command = new SqlCommand(Query, _connection);

                Command.Parameters.AddWithValue("idUser", idUser);
                if(idOrder != 0) Command.Parameters.AddWithValue("idOrder", idOrder);

                Command.ExecuteNonQuery();
                _connection.Close();

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

    }
}
