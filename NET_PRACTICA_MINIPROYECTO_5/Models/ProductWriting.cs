namespace NET_PRACTICA_MINIPROYECTO_5.Models
{
    public class ProductWriting
    {
        public int IdProduct { get; set; }
        public string Author { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int IdCategory { get; set; }
        public int Stock { get; set; }
        public decimal Price { get; set; }
        public string ImageRoute { get; set; } = string.Empty;

        public int OrderQuantity { get; set; }
        public int IdOrder { get; set; }
        public IFormFile? FormFile { get; set; }


        public string Date { get; set; } = string.Empty;

    }
}
