using System.Collections.Generic;

namespace EShop.Dtos
{
    public class CategoryResponseDto
    {
        public int CategoryId { get; set; }

        public string? CategoryName { get; set; }

        // Optional: List of products in the category
        public List<ProductResponseDto>? Products { get; set; }
    }
}
