using NET_PRACTICA_MINIPROYECTO_5.Interfaces;
using NET_PRACTICA_MINIPROYECTO_5.Models;

namespace NET_PRACTICA_MINIPROYECTO_5.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductsRepository _productsRepository;
        private readonly IBlobRepository _blobRepository;

        public ProductService(IProductsRepository productsRepository, IBlobRepository blobRepository)
        {
            _productsRepository = productsRepository;
            _blobRepository = blobRepository;
        }

        public async void CreateProducts(ProductReading products)
        {
            //Upload the photo to Azure BlobStroge and get the uri to storage it
            string Uri = await _blobRepository.UploadBlobFile(products.ImageRute.FilePath, products.ImageRute.FileName);

            _productsRepository.CreateNew(products, Uri);
        }

        public List<ProductWriting> GetProductsbyFilter(ProductFilter productFilter)
        {
            try
            {
                var Products = _productsRepository.GetAllbyFilter(productFilter.CuantityFilter, productFilter.CategoryFilter);

                return Products;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ocurred an error {ex.Message}");
            }

        }

        public async Task UpdateProducts(ProductReading products)
        {
            try
            {
                //If we want to change the photo, we need to upload the photo, if not then the image rute will be empty
                //and it wont execute the logic for that.
                if (products.ImageRute.FilePath != "" && products.ImageRute.FilePath != "")
                {
                    
                    string NewUri = await _blobRepository.UploadBlobFile(products.ImageRute.FilePath, products.ImageRute.FileName);

                    //Delete prev photo of Azure
                    var PrevUri = _productsRepository.GetImageUri(products.IdProduct);
                    _blobRepository.DeleteBlob(PrevUri);

                    //Update AnyThing
                    _productsRepository.Update(products, NewUri);
                }
                else
                {
                    //Update the product with the new data, depends of witch data we want to change
                    var ActualUri = _productsRepository.GetImageUri(products.IdProduct);
                    _productsRepository.Update(products, ActualUri);
                }


            }
            catch(Exception ex) 
            {
                throw new Exception(ex.Message);
            }


        }

        public async Task<BlobObject> GetImages(int idProduct)
        {
            string uri = _productsRepository.GetImageUri(idProduct);

            var blobObject =  await _blobRepository.GetBlobFile(uri);

            return blobObject;
        }
    }
}
