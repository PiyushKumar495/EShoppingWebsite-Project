using EShop.Dtos;
using EShop.Models;
using EShop.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EShop.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IGenericRepository<Product> _repository;
        private readonly IGenericRepository<Category> _categoryRepository; // Added category repository

        public ProductController(IGenericRepository<Product> repository, IGenericRepository<Category> categoryRepository)
        {
            _repository = repository;
            _categoryRepository = categoryRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _repository.GetAllAsync();
            var categories = await _categoryRepository.GetAllAsync();

            var result = products.Select(p => new ProductResponseDto
            {
                ProductId = p.ProductId,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                CategoryName = categories.FirstOrDefault(c => c.CategoryId == p.CategoryId)?.CategoryName ?? "Unknown"
            });

            return Ok(result);
        }
        [AllowAnonymous]
        [HttpGet("by-name")]
        public async Task<IActionResult> GetByName([FromQuery] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("Product name is required.");

            var products = await _repository.GetAllAsync();
            var categories = await _categoryRepository.GetAllAsync();

            var product = products.FirstOrDefault(p =>
                p.Name?.Equals(name, StringComparison.OrdinalIgnoreCase) == true);

            if (product == null) return NotFound("Product not found.");

            var Dto = new ProductResponseDto
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                CategoryName = categories.FirstOrDefault(c => c.CategoryId == product.CategoryId)?.CategoryName ?? "Unknown"
            };

            return Ok(Dto);
        }


        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(ProductCreateDto Dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var category = (await _categoryRepository.GetAllAsync())
                .FirstOrDefault(c => c.CategoryName?.Equals(Dto.CategoryName, StringComparison.OrdinalIgnoreCase) == true);

            if (category == null)
                return BadRequest("Category not found.");

            var product = new Product
            {
                Name = Dto.Name,
                Description = Dto.Description,
                Price = Dto.Price,
                StockQuantity = Dto.StockQuantity,
                CategoryId = category.CategoryId
            };

            await _repository.AddAsync(product);

            var response = new ProductResponseDto
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                CategoryName = category.CategoryName // Include category name in response
            };

            return CreatedAtAction(nameof(GetByName), new { id = product.ProductId }, response);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("by-name")]
        public async Task<IActionResult> UpdateByName([FromQuery] string name, [FromBody] ProductUpdateDto Dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("Product name is required.");

            var products = await _repository.GetAllAsync();
            var existing = products.FirstOrDefault(p =>
                p.Name?.Equals(name, StringComparison.OrdinalIgnoreCase) == true);

            if (existing == null)
                return NotFound("Product not found.");

            var category = (await _categoryRepository.GetAllAsync())
                .FirstOrDefault(c => c.CategoryName?.Equals(Dto.CategoryName, StringComparison.OrdinalIgnoreCase) == true);

            if (category == null)
                return BadRequest("Category not found.");

            existing.Name = Dto.Name;
            existing.Description = Dto.Description;
            existing.Price = Dto.Price;
            existing.StockQuantity = Dto.StockQuantity;
            existing.CategoryId = category.CategoryId;

            await _repository.UpdateAsync(existing);
            return NoContent();
        }


        [Authorize(Roles = "Admin")]
        [HttpDelete("by-name")]
        public async Task<IActionResult> DeleteByName([FromQuery] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("Product name is required.");

            var products = await _repository.GetAllAsync();
            var existing = products.FirstOrDefault(p =>
                p.Name?.Equals(name, StringComparison.OrdinalIgnoreCase) == true);

            if (existing == null)
                return NotFound("Product not found.");

            await _repository.DeleteAsync(existing.ProductId);
            return NoContent();
        }

    }
}
