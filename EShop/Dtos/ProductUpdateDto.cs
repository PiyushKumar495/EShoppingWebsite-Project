using System.ComponentModel.DataAnnotations;

namespace EShop.Dtos
{
    public class ProductUpdateDto
    {
        [Required, MaxLength(100)]
        public string? Name { get; set; }

        [Required, MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        [Range(0.01, 1000000)]
        public decimal Price { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }

        [Required, MaxLength(100)]
        public required string CategoryName { get; set; }
    }
}
