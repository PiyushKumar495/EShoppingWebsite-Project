using Microsoft.AspNetCore.Mvc;
using Moq;
using EShop.Controllers;
using EShop.Dtos;
using EShop.Models;
using EShop.Repositories;

namespace EShop.Tests;

[TestFixture]
public class ProductControllerTests
{
    private ProductController _controller;
    private Mock<IGenericRepository<Product>> _mockProductRepository;
    private Mock<IGenericRepository<Category>> _mockCategoryRepository;

    [SetUp]
    public void Setup()
    {
        _mockProductRepository = new Mock<IGenericRepository<Product>>();
        _mockCategoryRepository = new Mock<IGenericRepository<Category>>();
        _controller = new ProductController(_mockProductRepository.Object, _mockCategoryRepository.Object);
    }

    [Test]
    public async Task GetAll_ReturnsOkWithProducts()
    {
        var products = new List<Product>
        {
            new Product { ProductId = 1, Name = "Laptop", Price = 1000, CategoryId = 1 },
            new Product { ProductId = 2, Name = "Phone", Price = 500, CategoryId = 1 }
        };
        var categories = new List<Category>
        {
            new Category { CategoryId = 1, CategoryName = "Electronics" }
        };

        _mockProductRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(products);
        _mockCategoryRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(categories);

        var result = await _controller.GetAll();

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
    }

    [Test]
    public async Task GetByName_ValidName_ReturnsOk()
    {
        var products = new List<Product>
        {
            new Product { ProductId = 1, Name = "Laptop", Price = 1000, CategoryId = 1 }
        };
        var categories = new List<Category>
        {
            new Category { CategoryId = 1, CategoryName = "Electronics" }
        };

        _mockProductRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(products);
        _mockCategoryRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(categories);

        var result = await _controller.GetByName("Laptop");

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
    }

    [Test]
    public async Task GetByName_EmptyName_ReturnsBadRequest()
    {
        var result = await _controller.GetByName("");

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badResult = result as BadRequestObjectResult;
        Assert.That(badResult?.Value, Is.EqualTo("Product name is required."));
    }

    [Test]
    public async Task GetByName_NotFound_ReturnsNotFound()
    {
        _mockProductRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Product>());
        _mockCategoryRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Category>());

        var result = await _controller.GetByName("NonExistent");

        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        var notFoundResult = result as NotFoundObjectResult;
        Assert.That(notFoundResult?.Value, Is.EqualTo("Product not found."));
    }

    [Test]
    public async Task Create_ValidDto_ReturnsCreated()
    {
        var dto = new ProductCreateDto
        {
            Name = "New Product",
            Description = "Description",
            Price = 100,
            StockQuantity = 10,
            CategoryName = "Electronics"
        };
        var categories = new List<Category>
        {
            new Category { CategoryId = 1, CategoryName = "Electronics" }
        };

        _mockCategoryRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(categories);
        _mockProductRepository.Setup(r => r.AddAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);

        var result = await _controller.Create(dto);

        Assert.That(result, Is.InstanceOf<CreatedAtActionResult>());
    }

    [Test]
    public async Task Create_CategoryNotFound_ReturnsBadRequest()
    {
        var dto = new ProductCreateDto
        {
            Name = "New Product",
            Description = "Test Description",
            Price = 100,
            StockQuantity = 10,
            CategoryName = "NonExistent"
        };
        _mockCategoryRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Category>());

        var result = await _controller.Create(dto);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badResult = result as BadRequestObjectResult;
        Assert.That(badResult?.Value, Is.EqualTo("Category not found."));
    }

    [Test]
    public async Task UpdateByName_ValidData_ReturnsNoContent()
    {
        var products = new List<Product>
        {
            new Product { ProductId = 1, Name = "Laptop", CategoryId = 1 }
        };
        var categories = new List<Category>
        {
            new Category { CategoryId = 1, CategoryName = "Electronics" }
        };
        var dto = new ProductUpdateDto
        {
            Name = "Updated Laptop",
            Description = "Updated",
            Price = 1200,
            StockQuantity = 5,
            CategoryName = "Electronics"
        };

        _mockProductRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(products);
        _mockCategoryRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(categories);
        _mockProductRepository.Setup(r => r.UpdateAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);

        var result = await _controller.UpdateByName("Laptop", dto);

        Assert.That(result, Is.InstanceOf<NoContentResult>());
    }

    [Test]
    public async Task UpdateByName_EmptyName_ReturnsBadRequest()
    {
        var dto = new ProductUpdateDto { Name = "Updated", Description = "Updated", Price = 100, StockQuantity = 5, CategoryName = "Electronics" };

        var result = await _controller.UpdateByName("", dto);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task UpdateByName_ProductNotFound_ReturnsNotFound()
    {
        var dto = new ProductUpdateDto { Name = "Updated", Description = "Updated", Price = 100, StockQuantity = 5, CategoryName = "Electronics" };
        _mockProductRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Product>());

        var result = await _controller.UpdateByName("NonExistent", dto);

        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task UpdateByName_CategoryNotFound_ReturnsBadRequest()
    {
        var products = new List<Product>
        {
            new Product { ProductId = 1, Name = "Laptop" }
        };
        var dto = new ProductUpdateDto { Name = "Updated", Description = "Updated", Price = 100, StockQuantity = 5, CategoryName = "NonExistent" };

        _mockProductRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(products);
        _mockCategoryRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Category>());

        var result = await _controller.UpdateByName("Laptop", dto);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task DeleteByName_ValidName_ReturnsNoContent()
    {
        var products = new List<Product>
        {
            new Product { ProductId = 1, Name = "Laptop" }
        };
        _mockProductRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(products);
        _mockProductRepository.Setup(r => r.DeleteAsync(It.IsAny<int>())).Returns(Task.CompletedTask);

        var result = await _controller.DeleteByName("Laptop");

        Assert.That(result, Is.InstanceOf<NoContentResult>());
    }

    [Test]
    public async Task DeleteByName_EmptyName_ReturnsBadRequest()
    {
        var result = await _controller.DeleteByName("");

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task DeleteByName_NotFound_ReturnsNotFound()
    {
        _mockProductRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Product>());

        var result = await _controller.DeleteByName("NonExistent");

        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task Create_EmptyName_ReturnsBadRequest()
    {
        var dto = new ProductCreateDto
        {
            Name = "",
            Description = "Test",
            Price = 100,
            StockQuantity = 10,
            CategoryName = "Electronics"
        };
        var result = await _controller.Create(dto);
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task Create_DuplicateName_ReturnsBadRequest()
    {
        var products = new List<Product>
        {
            new Product { ProductId = 1, Name = "Laptop" }
        };
        var dto = new ProductCreateDto
        {
            Name = "Laptop",
            Description = "Test",
            Price = 100,
            StockQuantity = 10,
            CategoryName = "Electronics"
        };
        _mockProductRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(products);

        var result = await _controller.Create(dto);
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task GetAll_HandlesEmptyCategories()
    {
        var products = new List<Product>
        {
            new Product { ProductId = 1, Name = "Laptop", Price = 1000, CategoryId = 1 }
        };
        _mockProductRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(products);
        _mockCategoryRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Category>());

        var result = await _controller.GetAll();
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
    }

    [Test]
    public async Task UpdateByName_CallsUpdateAsync()
    {
        var products = new List<Product>
        {
            new Product { ProductId = 1, Name = "Laptop", CategoryId = 1 }
        };
        var categories = new List<Category>
        {
            new Category { CategoryId = 1, CategoryName = "Electronics" }
        };
        var dto = new ProductUpdateDto
        {
            Name = "Updated Laptop",
            Description = "Updated",
            Price = 1200,
            StockQuantity = 5,
            CategoryName = "Electronics"
        };
        _mockProductRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(products);
        _mockCategoryRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(categories);
        _mockProductRepository.Setup(r => r.UpdateAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);

        await _controller.UpdateByName("Laptop", dto);
        _mockProductRepository.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Once);
    }

    [Test]
    public async Task GetByName_HandlesUnknownCategory()
    {
        var products = new List<Product>
        {
            new Product { ProductId = 1, Name = "Laptop", Price = 1000, CategoryId = 999 }
        };
        _mockProductRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(products);
        _mockCategoryRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Category>());

        var result = await _controller.GetByName("Laptop");
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
    }
}