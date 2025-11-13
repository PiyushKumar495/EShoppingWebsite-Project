using EShop.Dtos;
using EShop.Models;
using EShop.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EShop.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly IGenericRepository<Category> _categoryRepository;

        public CategoryController(IGenericRepository<Category> categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        // Public: Get all categories with products
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await _categoryRepository.GetAllIncludingAsync(c => c.Products);

            var result = categories.Select(c => new CategoryResponseDto
            {
                CategoryId = c.CategoryId,
                CategoryName = c.CategoryName,
                Products = c.Products?.Select(p => new ProductResponseDto
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    CategoryName = c.CategoryName // Ensure CategoryName is included
                }).ToList()
            });

            return Ok(result);
        }

        // Public: Get category by Name
        [AllowAnonymous]
        [HttpGet("by-name")]
        public async Task<IActionResult> GetCategoryByName([FromQuery] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("Category name is required.");

            var categories = await _categoryRepository.GetAllIncludingAsync(c => c.Products);

            var category = categories.FirstOrDefault(c =>
                c.CategoryName?.Equals(name, StringComparison.OrdinalIgnoreCase) == true);

            if (category == null)
                return NotFound("Category not found.");

            var result = new CategoryResponseDto
            {
                CategoryId = category.CategoryId,
                CategoryName = category.CategoryName,
                Products = category.Products?.Select(p => new ProductResponseDto
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    CategoryName = category.CategoryName // Ensure CategoryName is included
                }).ToList()
            };

            return Ok(result);
        }


        // Admin: Create a new category
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryCreateDto Dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var category = new Category
            {
                CategoryName = Dto.CategoryName
            };

            await _categoryRepository.AddAsync(category);

            var response = new CategoryResponseDto
            {
                CategoryId = category.CategoryId,
                CategoryName = category.CategoryName,
                Products = new List<ProductResponseDto>() // empty list initially
            };

            return Ok(new { Message = "Category created successfully", Category = response });
        }

       
        [Authorize(Roles = "Admin")]
        [HttpPut("by-name")]
        public async Task<IActionResult> UpdateCategoryByName([FromQuery] string name, [FromBody] CategoryUpdateDto Dto)
        {
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("Category name is required.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var categories = await _categoryRepository.GetAllAsync();
            var existing = categories.FirstOrDefault(c =>
                c.CategoryName?.Equals(name, StringComparison.OrdinalIgnoreCase) == true);

            if (existing == null)
                return NotFound("Category not found");

            existing.CategoryName = Dto.CategoryName;

            await _categoryRepository.UpdateAsync(existing);

            return Ok(new
            {
                Message = "Category updated successfully",
                Category = new CategoryResponseDto
                {
                    CategoryId = existing.CategoryId,
                    CategoryName = existing.CategoryName
                }
            });
        }


        // Admin: Delete a category
        [Authorize(Roles = "Admin")]
        [HttpDelete("by-name")]
        public async Task<IActionResult> DeleteCategoryByName([FromQuery] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("Category name is required.");

            var categories = await _categoryRepository.GetAllAsync();
            var category = categories.FirstOrDefault(c =>
                c.CategoryName?.Equals(name, StringComparison.OrdinalIgnoreCase) == true);

            if (category == null)
                return NotFound("Category not found.");

            await _categoryRepository.DeleteAsync(category.CategoryId);
            return Ok("Category deleted successfully.");
        }
    }
}
