using NET_PRACTICA_MINIPROYECTO_5.Models;

namespace NET_PRACTICA_MINIPROYECTO_5.Interfaces
{
    public interface IProductsRepository
    {
        public List<ProductWriting> GetAllByFilter(string quantityFilter, string categoryFilter, string userFilter = "", bool isOrder = false);
        public void CreateNew(ProductReading product_Reading, string Uri);
        public void Update(ProductReading products, string? url);
        public void Delete(int IdProduct);
        public string VerifyUser(int IdUser, int IdProduct);
        public string GetImageUri(int ProductId);
        public string GetImagePublicId(int ProductId);


        public int GetTotalOrders(int idUser);
        public decimal GetTotalPriceOrders(int idUser);
        public void DeleteOrders(int idUser, int idProduct);
        public void CreateOrder(OrderModel orderModel);
    }
}
