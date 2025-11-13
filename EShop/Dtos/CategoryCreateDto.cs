using System.ComponentModel.DataAnnotations;

namespace EShop.Dtos
{
    public class CategoryCreateDto
    {
        [Required]
        [MaxLength(100)]
        public string? CategoryName { get; set; }
    }
}
