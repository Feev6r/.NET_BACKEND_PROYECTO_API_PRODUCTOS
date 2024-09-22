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

        [HttpGet]
        [Route("show")]
        public ActionResult GetProduts([FromQuery] ProductFilter productFilter)
        {
            try
            {
                List<ProductWriting> result = _productService.GetProductsbyFilter(productFilter);

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

                List<ProductWriting> result = _productService.GetProductsbyFilter(productFilter);

                return Ok(result);
            }
            catch 
            { 
                return StatusCode(500, $"Error while trying to get the user products");
            }

        }


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


    }
}
