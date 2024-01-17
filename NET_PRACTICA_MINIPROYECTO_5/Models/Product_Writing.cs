namespace NET_PRACTICA_MINIPROYECTO_5.Models
{
    public class Product_Writing
    {
        public int IdProduct { get; set; }
        public string Author { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int IdCategory { get; set; }
        public int Stock { get; set; }
        public decimal Price { get; set; }
        public string Date { get; set; } = string.Empty;
    }
    //La idea es tener 2 clases totalmente moldeadas para envio tanto del back al front y del front al back
    public class Product_Reading
    {
        public int IdProduct { get; set; }
        public int IdUser { get; set; } 
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int IdCategory { get; set; }
        public int Stock { get; set; }
        public decimal Price { get; set; }
        public DateTime Date { get; set; }
    }
}
