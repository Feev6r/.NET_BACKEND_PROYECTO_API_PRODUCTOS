using NET_PRACTICA_MINIPROYECTO_5.Models;

namespace NET_PRACTICA_MINIPROYECTO_5.Interfaces
{
    public interface IProductService
    {
        public List<ProductWriting> GetProductsByFilter(ProductFilter productFilter);

        public void CreateProducts(ProductReading products);

        public void UpdateProducts(ProductReading products);

        public void DeleteProduct(int IdUser, int IdProduct);

        public int TotalOrders(int idUser);

        public decimal TotalPrice(int idUser);

        public void DeleteOrder(int idUser, int idProduct);



        public Task<BlobObject> GetImages(int idProduct);
        public void MakeOrder(OrderModel orderModel);
    }
}
