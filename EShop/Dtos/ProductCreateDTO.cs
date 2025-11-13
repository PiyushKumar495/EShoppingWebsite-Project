using System.ComponentModel.DataAnnotations;

namespace EShop.Dtos
{
    public class ProductCreateDto
    {
        [Required, MaxLength(100)]
        public required string Name { get; set; }

        [Required, MaxLength(500)]
        public required string Description { get; set; }

        [Required]
        [Range(0.01, 1000000)]
        public decimal Price { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }

        [Required, MaxLength(100)]
        public string? CategoryName { get; set; } 
    }
}
