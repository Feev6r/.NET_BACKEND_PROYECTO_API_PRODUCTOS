using NET_PRACTICA_MINIPROYECTO_5.Interfaces;
using NET_PRACTICA_MINIPROYECTO_5.Models;
using System.Reflection;

namespace NET_PRACTICA_MINIPROYECTO_5.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductsRepository _productsRepository;
        private readonly IBlobRepository _blobRepository;
        private readonly ICloudinaryImgRepository _cloudinaryImgRepository;

        public ProductService(
            IProductsRepository productsRepository, 
            IBlobRepository blobRepository, 
            ICloudinaryImgRepository cloudinaryImgRepository)

        {
            _productsRepository = productsRepository;
            _blobRepository = blobRepository;
            _cloudinaryImgRepository = cloudinaryImgRepository;
        }

        public void CreateProducts(ProductReading products)
        {
            //Upload the photo to Azure BlobStroge and get the uri to storage it
            //string Uri = await _blobRepository.UploadBlobFile(products.BlobImage.FilePath, products.BlobImage.FileName);

            string url = _cloudinaryImgRepository.UploadImage(products.File!);

            _productsRepository.CreateNew(products, url);    

        }

        public List<ProductWriting> GetProductsbyFilter(ProductFilter productFilter)
        {
            try
            {
                var Products = _productsRepository.GetAllbyFilter(productFilter.CuantityFilter, productFilter.CategoryFilter, productFilter.UserFilter);

                return Products;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ocurred an error {ex.Message}");
            }

        }

        public void UpdateProducts(ProductReading products)
        {
            try
            {
                if (products.File != null) {

                    string prevImagePublicId = _productsRepository.getImagePublicId(products.IdProduct);
                    _cloudinaryImgRepository.DeleteImage(prevImagePublicId);

                    string newImagePublicId = _cloudinaryImgRepository.UploadImage(products.File);

                    _productsRepository.Update(products, newImagePublicId);

                }
                else
                {
                    _productsRepository.Update(products, null);
                }

                //azure stuff

                //If we want to change the photo, we need to upload the photo, if not, then the image rute will be empty
                //and it wont execute the logic for that.
                /*                if (products.BlobImage.FilePath != "" && products.BlobImage.FilePath != "")
                {

                    string NewUri = await _blobRepository.UploadBlobFile(products.BlobImage.FilePath, products.BlobImage.FileName);

                    //Delete prev photo of Azure
                    var PrevUri = _productsRepository.GetImageUri(products.IdProduct);
                    _blobRepository.DeleteBlob(PrevUri);

 
                    _productsRepository.Update(products, NewUri);
                }
                else
                {
                    //Update the product with the new data, depends of witch data we want to change
                    var ActualUri = _productsRepository.GetImageUri(products.IdProduct);
                    _productsRepository.Update(products, ActualUri);
                }*/


            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }


        }

        public async Task<BlobObject> GetImages(int idProduct)
        {
            string uri = _productsRepository.GetImageUri(idProduct);

            var blobObject = await _blobRepository.GetBlobFile(uri);

            return blobObject;
        }

        private void CanUserDeleteProduct(int IdUser, int IdProduct)
        {
            try
            {
                if (_productsRepository.VerifyUser(IdUser, IdProduct) == "0")
                {
                    throw new UnauthorizedAccessException();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void DeleteProduct(int IdUser, int IdProduct)
        {


            CanUserDeleteProduct(IdUser, IdProduct);

            string ImagePublicId = _productsRepository.getImagePublicId(IdProduct);
            _cloudinaryImgRepository.DeleteImage(ImagePublicId);

            _productsRepository.Delete(IdProduct);

            //_blobRepository.DeleteBlob(ImageUri);

        }
    }
}
