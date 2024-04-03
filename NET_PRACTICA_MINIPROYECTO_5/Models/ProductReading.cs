using System.ComponentModel.DataAnnotations;

namespace NET_PRACTICA_MINIPROYECTO_5.Models
{
    //La idea es tener 2 clases totalmente moldeadas para envio tanto del back al front y del front al back
    //writing cuando se envia al cliente (recivice info de sql y se envia al cliente)
    //reading cunaod se recive del cliente (recive info del cliente y envia a sql)
    public class ProductReading
    {
        public int IdProduct { get; set; }
        public int IdUser { get; set; }

        [StringLength(75)]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;
        public int IdCategory { get; set; }
        public int Stock { get; set; }
        public decimal Price { get; set; }
       
        public DateTime Date { get; }

        public BlobContentModel ImageRute { get; set;} = new BlobContentModel();

        public ProductReading()
        {
            Date = DateTime.Today;
        }
    }
}
