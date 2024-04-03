using NET_PRACTICA_MINIPROYECTO_5.Models;

namespace NET_PRACTICA_MINIPROYECTO_5.Interfaces
{
    public interface IProductService
    {
        public List<ProductWriting> GetProductsbyFilter(ProductFilter productFilter);

        public void CreateProducts(ProductReading products);

        public Task UpdateProducts(ProductReading products);

        public Task<BlobObject> GetImages(int idProduct);
    }
}
