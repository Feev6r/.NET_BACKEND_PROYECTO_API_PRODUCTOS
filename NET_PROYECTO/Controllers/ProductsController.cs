using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NET_PRACTICA_MINIPROYECTO_5.Interfaces;
using NET_PRACTICA_MINIPROYECTO_5.Models;

namespace NET_PRACTICA_MINIPROYECTO_5.Controllers
{
    [Route("Products")]
    [ApiController]
    public class ProductsController : ControllerBase
    {

        #region variables and injections 
        private readonly ILogger<ProductsController> _logger;
        private readonly IProductService _productService;
        private readonly ICloudinaryImgRepository _cloudinary;
        private readonly IAntiforgery _antiforgery;

        public ProductsController(
            ILogger<ProductsController> logger,
            IProductService productService,
            ICloudinaryImgRepository cloudinary,
            IAntiforgery antiforgery
            )
        {
            _logger = logger;
            _productService = productService;
            _cloudinary = cloudinary;
            _antiforgery = antiforgery;
        }
        #endregion

        [HttpGet]
        [Route("show")]
        public ActionResult GetProducts([FromQuery] ProductFilter productFilter)
        {
            try
            {
                List<ProductWriting> result = _productService.GetProductsByFilter(productFilter);

                return Ok(result);
            }
            catch
            {
                return StatusCode(500, new { message = "Error while trying to get the products" });
            }

        }

        [HttpPost("create"), Authorize]
        public ActionResult CreateProduct([FromForm] ProductReading Products)
        {

            try
            {
                Products.IdUser = int.Parse(HttpContext.User.FindFirst("UserId")!.Value);
                _productService.CreateProducts(Products);

                return Ok();
            }
            catch
            {
                return StatusCode(500, "Error while trying to create the product");
            }

        }

        [HttpPut("edit{productId}"), Authorize]
        public ActionResult EditProducts(int productId, [FromForm] ProductReading Products)
        {
            try
            {
                Products.IdProduct = productId;
                _productService.UpdateProducts(Products);

                return Ok();
            }
            catch
            {
                return StatusCode(500, $"Error while trying to edit the product");
            }

        }

        [HttpDelete("delete{productId}"), Authorize]
        public ActionResult DeleteProduct(int productId)
        {

            try
            {
                int IdUser = int.Parse(HttpContext.User.FindFirst("UserId")!.Value);

                _productService.DeleteProduct(IdUser, productId);

                return Ok();
            }
            catch
            {
                return StatusCode(500, $"Error while trying to delete the product");
            }

        }

        [HttpGet("user"), Authorize]
        public ActionResult GetUserProducts([FromQuery] ProductFilter productFilter)
        {
            try
            {
                productFilter.UserFilter = HttpContext.User.FindFirst("UserId")!.Value;

                List<ProductWriting> result = _productService.GetProductsByFilter(productFilter);

                return Ok(result);
            }
            catch 
            { 
                return StatusCode(500, $"Error while trying to get the user products");
            }

        }





        [HttpPost("orders/make"), Authorize]
        public ActionResult createOrder([FromBody] OrderModel orderModel)
        {
            try
            {
                orderModel.IdUser = int.Parse(HttpContext.User.FindFirst("UserId")!.Value);
                _productService.MakeOrder(orderModel);
                return Ok();
            }
            catch
            {
                return StatusCode(500, new { message = "Error while trying to create an order" });
            }
        }

        [HttpGet("orders/total"), Authorize]
        public ActionResult getTotalOrders()
        {
            try
            {
                int idUser = int.Parse(HttpContext.User.FindFirst("UserId")!.Value);
                return Ok(_productService.TotalOrders(idUser));
            }
            catch
            {
                return StatusCode(500, new { message = "Error while trying to get the total orders" });

            }
        }

        [HttpGet("orders/totalPrice"), Authorize]
        public ActionResult getTotalPrice()
        {
            try
            {
                int idUser = int.Parse(HttpContext.User.FindFirst("UserId")!.Value);
                return Ok(_productService.TotalPrice(idUser));
            }
            catch
            {
                return StatusCode(500, new { message = "Error while trying to get the total price" });
            }

        }


        [HttpDelete("orders/delete{idOrder}"), Authorize]
        public ActionResult deleteOrders(int idOrder = 0)
        {
            try
            {
                int idUser = int.Parse(HttpContext.User.FindFirst("UserId")!.Value);
                _productService.DeleteOrder(idUser, idOrder);
                return Ok();                
            }
            catch 
            {
                return StatusCode(500, new { message = "Error while trying to delete some orders" });
            }

        }


        #region DebugStuff

        //Testing and debbuging method
        [HttpGet, Authorize]
        [Route("test")]
        public ActionResult Test()
        {
            try
            {
                return Ok(new { message = $"All good" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"ERROR AT TEST {ex.Message}" });

            }
        }

        //returns a blob back, azure stuff (former approach)
        [HttpGet]
        [Route("images/{idPublicacion}")]
        public ActionResult Image(int idPublicacion)
        {
            try
            {

                //_cloudinary.uploadImage("C:\\Users\\FernandoArandano\\Desktop\\sis.jpg");
                return Ok();
                //return Ok(_cloudinary.GetImage());    
                /*              
                 *              
                                var blobObject = await _productService.GetImages(idPublicacion);

                                return File(blobObject.Content!, blobObject.ContentType!);*/
            }
            catch
            {
                return StatusCode(500, "Error while trying to get the image");
            }

        }

        #endregion
    }
}

