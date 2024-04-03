using NET_PRACTICA_MINIPROYECTO_5.Models;

namespace NET_PRACTICA_MINIPROYECTO_5.Interfaces
{
    public interface IProductsRepository
    {
        public List<ProductWriting> GetAllbyFilter(string cuantityFilter, string categoryFilter);
        public void CreateNew(ProductReading product_Reading, string Uri);
        public void Update(ProductReading products, string Uri);
        public void Delete(ProductReading products);
        public string GetImageUri(int ProductId);
    }
}
