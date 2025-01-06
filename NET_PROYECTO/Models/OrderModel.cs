namespace NET_PRACTICA_MINIPROYECTO_5.Models
{
    public class OrderModel
    {

        
        public int Quantity { get; set; }
        public DateTime Date { get; set; }
        public int IdUser { get; set; }
        public int IdProduct { get; set; }

        public OrderModel()
        {
            Date = DateTime.Today;
        }

    }
}
