using NET_PRACTICA_MINIPROYECTO_5.Models;

namespace NET_PRACTICA_MINIPROYECTO_5.Interfaces
{
    public interface IProductsRepository
    {
        public List<ProductWriting> GetAllbyFilter(string cuantityFilter, string categoryFilter, string UserFilter = "");
        public void CreateNew(ProductReading product_Reading, string Uri);
        public void Update(ProductReading products, string? url);
        public void Delete(int IdProduct);
        public string VerifyUser(int IdUser, int IdProduct);

        public string GetImageUri(int ProductId);

        public string getImagePublicId(int ProductId);
    }
}
