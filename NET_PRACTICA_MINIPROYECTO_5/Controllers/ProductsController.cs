using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NET_PRACTICA_MINIPROYECTO_5.Attributes;
using NET_PRACTICA_MINIPROYECTO_5.Models;
using NET_PRACTICA_MINIPROYECTO_5.Services;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Text.Json;

namespace NET_PRACTICA_MINIPROYECTO_5.Controllers
{
    [Route("Show")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly SqlConnection _connection;
        private readonly IConfiguration _configuration;
        private readonly IAntiforgery _antiforgery;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(
            IConfiguration configuration
            , IDbConnectionFactory dbConnectionFactory,
            IAntiforgery antiforgery,
            ILogger<ProductsController> logger,
            IRefreshTokenService refreshTokenService

            )
        {
            _configuration = configuration;
            _connection = dbConnectionFactory.CreateConnection();
            _antiforgery = antiforgery;
            _logger = logger;
            _refreshTokenService = refreshTokenService;
        }

        [HttpGet, Authorize, ValidateTokensCsrf]
        [Route("Productos")]
        public async Task<IActionResult> MostrarProductos()
        {

            List<Product_Writing> productos = new();

            string consulta = "SELECT products.*, " +
                "categories.Name AS Category, " +
                "users.Name AS Author " +
                "FROM products " +
                "LEFT JOIN " +
                "categories ON products.idCategory = categories.idCategory " +
                "LEFT JOIN " +
                "users ON products.idUser = users.idUser;";



            try
            {
                await _connection.OpenAsync();

                SqlCommand sqlCommand = new(consulta, _connection);

                SqlDataReader reader = await sqlCommand.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    productos.Add(new Product_Writing
                    {
                        IdProduct = reader.GetInt32("idProduct"),
                        Author = reader.GetString("Author"),
                        Title = reader.GetString("Title"),
                        Description = reader.GetString("Description"),
                        Stock = reader.GetInt32("Stock"),
                        Price = reader.GetDecimal("Price"),
                        Date = reader.GetDateTime("Date").ToString("yyyy-MM-dd"),
                        Category = reader.GetString("Category"), //necestamos tanto la categoria
                        IdCategory = reader.GetInt32("idCategory") //como el id porque es lo que identifica la categoria (para proximas peticiones)
                    });
                }

                await reader.CloseAsync();

                return Ok(productos);
            }
            catch (AntiforgeryValidationException)
            {
                return BadRequest("La validación del token CSRF falló.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            finally
            {
                await _connection.CloseAsync();
            }

        }

        [HttpPost]
        [Authorize]
        [ValidateTokensCsrf]
        [Route("crearProducto")]
        public async Task<IActionResult> CrearProducto([FromBody] dynamic product)
        {

            string consulta = "INSERT INTO Proyecto_1.dbo.products(Title, Description, Stock, Price, Date, idUser, idCategory) " +
                "VALUES(@Title, @Description, @Stock, @Price, @Date, @IdUser, @IdCategory)";

            try
            {
                await _connection.OpenAsync();

                SqlCommand sqlCommand = new(consulta, _connection);

                Product_Reading product_Reading = JsonSerializer.Deserialize<Product_Reading>(product.ToString());

                sqlCommand.Parameters.AddWithValue("Title", product_Reading.Title);
                sqlCommand.Parameters.AddWithValue("Description", product_Reading.Description);
                sqlCommand.Parameters.AddWithValue("Stock", product_Reading.Stock);
                sqlCommand.Parameters.AddWithValue("Price", product_Reading.Price);
                sqlCommand.Parameters.AddWithValue("Date", product_Reading.Date);
                sqlCommand.Parameters.AddWithValue("idUser", product_Reading.IdUser);
                sqlCommand.Parameters.AddWithValue("idCategory", product_Reading.IdCategory);

            }
            catch (JsonException jex)
            {
                return BadRequest(jex.Path);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            finally { await _connection.CloseAsync(); }

            return Ok();
        }

        [HttpPost, Authorize, ValidateTokensCsrf]
        [Route("test")]
        public IActionResult Test()
        {

            return Ok("Bien :D");
        }


    }
}
