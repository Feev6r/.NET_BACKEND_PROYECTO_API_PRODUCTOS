using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NET_PRACTICA_MINIPROYECTO_5.Attributes;
using NET_PRACTICA_MINIPROYECTO_5.Interfaces;
using NET_PRACTICA_MINIPROYECTO_5.Models;

namespace NET_PRACTICA_MINIPROYECTO_5.Controllers
{
    [Route("Show")]
    [ApiController]
    public class ProductsController : ControllerBase
    {

        private readonly ILogger<ProductsController> _logger;
        private readonly IProductService _productService;


        public ProductsController(
            ILogger<ProductsController> logger,
            IProductService productService
            )
        {
            _logger = logger;
            _productService = productService;
        }

        [HttpGet]
        [Route("products")]
        public ActionResult GetProduts([FromQuery] ProductFilter productFilter)
        {
            try
            {
                List<ProductWriting> result = _productService.GetProductsbyFilter(productFilter);


                return Ok(result);

            }
            catch 
            {
                return StatusCode(500, new { message = "Error while trying to get the products"});
            }

        }


        [HttpGet]
        [Route("images/{idPublicacion}")]
        public async Task<ActionResult> Image(int idPublicacion)
        {

            try
            {
                var blobObject = await _productService.GetImages(idPublicacion);

                return File(blobObject.Content!, blobObject.ContentType!);
            }
            catch
            {
                return StatusCode(500, "Error while trying to get the image");
            }

        }


        [HttpPost("createProduct"), Authorize, TokenCsrfGeneration]
        public ActionResult CreateProducts([FromBody] ProductReading Products)
        {

            try
            {
                _productService.CreateProducts(Products);

                return Ok();
            }
            catch
            {
                return StatusCode (500, "Error while trying to create the product");
            }

        }

        [HttpPut("editProducts"), Authorize, TokenCsrfGeneration]
        public async Task<ActionResult> EditProducts([FromBody] ProductReading Products)
        {
            try
            {
                await _productService.UpdateProducts(Products);

                return Ok();
            }
            catch
            {
                return StatusCode(500, $"Error while trying to edit the product");
            }

        }


        //Testing and debbuging method
        [HttpPost, TokenCsrfGeneration]
        [Route("test")]
        public ActionResult Test()
        {
            try
            {
                return Ok(new { message = $"All good" });

            }
            catch(Exception ex) 
            {
                return BadRequest(new {message = $"ERROR AT TEST {ex.Message}"});

            }
        }

    }
}


// C:\\Users\\FernandoArandano\\source\\repos\\DevProgrammingPractice\\PRACTICAS-ASP.NETCORE\\NET_PRACTICA_MINIPROYECTO_5\\NET_PRACTICA_MINIPROYECTO_5\\Images

//[HttpGet]
//[Route("Images/{idPublicacion}")]
//public ActionResult a(int idPublicacion)
// {
//string nombreImagen = "";
/*
            if (idPublicacion == 0)
            {
                //nombreImagen = "Banana.jpg";
            }
            else if(idPublicacion == 1)
            {
               // nombreImagen = "Brocoli.jpg";
            }

            var containerClient = _blobServiceClient.GetBlobContainerClient("productsimages");
            var blobClient = containerClient.GetBlobClient("image1");
            var response =  blobClient.DownloadContent();

            var properties = blobClient.GetProperties();


            // Convertir BinaryData a byte[]
            byte[] imageData =  response.Value.Content.ToArray();

            return File(imageData, properties.Value.ContentType);*/
/*
            string directorioImagenes = Path.Combine(Directory.GetCurrentDirectory(), "Images");
            string rutaImagen = Path.Combine(directorioImagenes, nombreImagen);

            if (!System.IO.File.Exists(rutaImagen))
            {
                return BadRequest(new { message = rutaImagen }); // Retorna un código de estado 404 si la imagen no se encuentra
            }

            byte[] bytesImagen = System.IO.File.ReadAllBytes(rutaImagen);



            return File(bytesImagen, "image/jpeg");*/


//  return Ok();

// }