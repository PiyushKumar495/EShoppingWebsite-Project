using System.ComponentModel.DataAnnotations;

namespace EShop.Dtos
{
    public class AdDtoCartDto
    {
        [Required]
        public string? ProductName { get; set; } // Changed to accept ProductName instead of ProductId

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }
    }
}
