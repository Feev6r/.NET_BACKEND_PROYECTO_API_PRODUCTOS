using System.ComponentModel.DataAnnotations;

namespace NET_PRACTICA_MINIPROYECTO_5.Models
{
    public class User
    {
        public int Id { get; set; }
        
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [StringLength(255)]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Not valid email")]
        public string Email { get; set; } = string.Empty;

        [StringLength(100)]
        public string Password { get; set; } = string.Empty;

    }
}
