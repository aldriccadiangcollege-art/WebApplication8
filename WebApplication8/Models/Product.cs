using System.ComponentModel.DataAnnotations;

namespace WebApplication8.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Name { get; set; } = "";

        [Range(0.01, 1000000)]
        public decimal Price { get; set; }
    }
}