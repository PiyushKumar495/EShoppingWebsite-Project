using Microsoft.AspNetCore.Mvc;
using Moq;
using EShop.Controllers;
using EShop.Dtos;
using EShop.Models;
using EShop.Repositories;

namespace EShop.Tests;

[TestFixture]
public class CategoryControllerTests
{
    private CategoryController _controller;
    private Mock<IGenericRepository<Category>> _mockRepository;

    [SetUp]
    public void Setup()
    {
        _mockRepository = new Mock<IGenericRepository<Category>>();
        _controller = new CategoryController(_mockRepository.Object);
    }

    [Test]
    public async Task GetAllCategories_ReturnsOkWithCategories()
    {
        var categories = new List<Category>
        {
            new Category { CategoryId = 1, CategoryName = "Electronics", Products = [] },
            new Category { CategoryId = 2, CategoryName = "Books", Products = [] }
        };
        _mockRepository.Setup(r => r.GetAllIncludingAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Category, object>>>()))
            .ReturnsAsync(categories);

        var result = await _controller.GetAllCategories();

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
    }

    [Test]
    public async Task GetCategoryByName_ValidName_ReturnsOk()
    {
        var categories = new List<Category>
        {
            new Category { CategoryId = 1, CategoryName = "Electronics", Products = [] }
        };
        _mockRepository.Setup(r => r.GetAllIncludingAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Category, object>>>()))
            .ReturnsAsync(categories);

        var result = await _controller.GetCategoryByName("Electronics");

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
    }

    [Test]
    public async Task GetCategoryByName_EmptyName_ReturnsBadRequest()
    {
        var result = await _controller.GetCategoryByName("");

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badResult = result as BadRequestObjectResult;
        Assert.That(badResult?.Value, Is.EqualTo("Category name is required."));
    }

    [Test]
    public async Task GetCategoryByName_NotFound_ReturnsNotFound()
    {
        _mockRepository.Setup(r => r.GetAllIncludingAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Category, object>>>()))
            .ReturnsAsync(new List<Category>());

        var result = await _controller.GetCategoryByName("NonExistent");

        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        var notFoundResult = result as NotFoundObjectResult;
        Assert.That(notFoundResult?.Value, Is.EqualTo("Category not found."));
    }

    [Test]
    public async Task CreateCategory_ValidDto_ReturnsOk()
    {
        var dto = new CategoryCreateDto { CategoryName = "New Category" };
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Category>())).Returns(Task.CompletedTask);

        var result = await _controller.CreateCategory(dto);

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
    }

    [Test]
    public async Task UpdateCategoryByName_ValidData_ReturnsOk()
    {
        var categories = new List<Category>
        {
            new Category { CategoryId = 1, CategoryName = "Electronics" }
        };
        var dto = new CategoryUpdateDto { CategoryName = "Updated Electronics" };
        
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(categories);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Category>())).Returns(Task.CompletedTask);

        var result = await _controller.UpdateCategoryByName("Electronics", dto);

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
    }

    [Test]
    public async Task UpdateCategoryByName_EmptyName_ReturnsBadRequest()
    {
        var dto = new CategoryUpdateDto { CategoryName = "Updated" };

        var result = await _controller.UpdateCategoryByName("", dto);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task UpdateCategoryByName_NotFound_ReturnsNotFound()
    {
        var dto = new CategoryUpdateDto { CategoryName = "Updated" };
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Category>());

        var result = await _controller.UpdateCategoryByName("NonExistent", dto);

        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task DeleteCategoryByName_ValidName_ReturnsOk()
    {
        var categories = new List<Category>
        {
            new Category { CategoryId = 1, CategoryName = "Electronics" }
        };
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(categories);
        _mockRepository.Setup(r => r.DeleteAsync(It.IsAny<int>())).Returns(Task.CompletedTask);

        var result = await _controller.DeleteCategoryByName("Electronics");

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = result as OkObjectResult;
        Assert.That(okResult?.Value, Is.EqualTo("Category deleted successfully."));
    }

    [Test]
    public async Task DeleteCategoryByName_EmptyName_ReturnsBadRequest()
    {
        var result = await _controller.DeleteCategoryByName("");

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task DeleteCategoryByName_NotFound_ReturnsNotFound()
    {
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Category>());

        var result = await _controller.DeleteCategoryByName("NonExistent");

        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task CreateCategory_ValidDto_CallsAddAsync()
    {
        var dto = new CategoryCreateDto { CategoryName = "New Category" };
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Category>())).Returns(Task.CompletedTask);

        await _controller.CreateCategory(dto);

        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Category>()), Times.Once);
    }

    [Test]
    public async Task GetAllCategories_HandlesEmptyProductList()
    {
        var categories = new List<Category>
        {
            new Category { CategoryId = 1, CategoryName = "Electronics", Products = [] }
        };
        _mockRepository.Setup(r => r.GetAllIncludingAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Category, object>>>()))
            .ReturnsAsync(categories);

        var result = await _controller.GetAllCategories();

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
    }

    [Test]
    public async Task UpdateCategoryByName_CallsUpdateAsync()
    {
        var categories = new List<Category>
        {
            new Category { CategoryId = 1, CategoryName = "Electronics" }
        };
        var dto = new CategoryUpdateDto { CategoryName = "Updated" };
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(categories);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Category>())).Returns(Task.CompletedTask);

        await _controller.UpdateCategoryByName("Electronics", dto);

        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Category>()), Times.Once);
    }

    [Test]
    public async Task GetAllCategories_ReturnsCompleteData_WithProducts()
    {
        var categories = new List<Category>
        {
            new Category 
            { 
                CategoryId = 1, 
                CategoryName = "Electronics", 
                Products = new List<Product>
                {
                    new Product { ProductId = 1, Name = "Laptop", Description = "Gaming laptop", Price = 1000, StockQuantity = 5 }
                }
            }
        };
        _mockRepository.Setup(r => r.GetAllIncludingAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Category, object>>>()))
            .ReturnsAsync(categories);

        var result = await _controller.GetAllCategories();
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
    }

    [Test]
    public async Task GetCategoryByName_ReturnsCompleteData_WithProducts()
    {
        var categories = new List<Category>
        {
            new Category 
            { 
                CategoryId = 1, 
                CategoryName = "Electronics", 
                Products = new List<Product>
                {
                    new Product { ProductId = 1, Name = "Laptop", Description = "Gaming laptop", Price = 1000, StockQuantity = 5 }
                }
            }
        };
        _mockRepository.Setup(r => r.GetAllIncludingAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Category, object>>>()))
            .ReturnsAsync(categories);

        var result = await _controller.GetCategoryByName("Electronics");
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
    }

    [Test]
    public async Task GetCategoryByName_HandlesNullProducts()
    {
        var categories = new List<Category>
        {
            new Category { CategoryId = 1, CategoryName = "Electronics", Products = [] }
        };
        _mockRepository.Setup(r => r.GetAllIncludingAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Category, object>>>()))
            .ReturnsAsync(categories);

        var result = await _controller.GetCategoryByName("Electronics");
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
    }

    [Test]
    public async Task GetCategoryByName_HandlesWhitespaceInput()
    {
        var result = await _controller.GetCategoryByName("   ");
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }
}