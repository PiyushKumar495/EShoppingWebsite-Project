using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using EShop.Controllers;
using EShop.Data;
using EShop.Dtos;
using EShop.Models;

namespace EShop.Tests;

[TestFixture]
public class AuthControllerTests
{
    private EshoppingDbContext _context;
    private AuthController _controller;
    private Mock<IConfiguration> _mockConfig;
    private Mock<IConfigurationSection> _mockJwtSection;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<EshoppingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new EshoppingDbContext(options);

        _mockConfig = new Mock<IConfiguration>();
        _mockJwtSection = new Mock<IConfigurationSection>();
        
        _mockJwtSection.Setup(x => x["Key"]).Returns("ThisIsASecretKeyForJWTTokenGeneration123456789");
        _mockJwtSection.Setup(x => x["Issuer"]).Returns("TestIssuer");
        _mockJwtSection.Setup(x => x["Audience"]).Returns("TestAudience");
        _mockConfig.Setup(x => x.GetSection("Jwt")).Returns(_mockJwtSection.Object);

        _controller = new AuthController(_context, _mockConfig.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    [Test]
    public async Task Register_ValidUser_ReturnsOk()
    {
        var dto = new UserRegisterDto
        {
            FullName = "Test User",
            Email = "test@example.com",
            Password = "password123",
            ConfirmPassword = "password123"
        };

        var result = await _controller.Register(dto);

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = result as OkObjectResult;
        Assert.That(okResult?.Value, Is.EqualTo("User registered successfully!"));
    }

    [Test]
    public async Task Register_DuplicateEmail_ReturnsBadRequest()
    {
        var existingUser = new User
        {
            FullName = "Existing User",
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"),
            Role = UserRole.Customer
        };
        _context.Users.Add(existingUser);
        await _context.SaveChangesAsync();

        var dto = new UserRegisterDto
        {
            FullName = "Test User",
            Email = "test@example.com",
            Password = "password123",
            ConfirmPassword = "password123"
        };

        var result = await _controller.Register(dto);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badResult = result as BadRequestObjectResult;
        Assert.That(badResult?.Value, Is.EqualTo("User with this email already exists."));
    }

    [Test]
    public async Task Login_ValidCredentials_ReturnsOkWithToken()
    {
        var user = new User
        {
            FullName = "Test User",
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
            Role = UserRole.Customer
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var dto = new UserLoginDto
        {
            Email = "test@example.com",
            Password = "password123"
        };

        var result = await _controller.Login(dto);

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = result as OkObjectResult;
        Assert.That(okResult?.Value, Is.Not.Null);
    }

    [Test]
    public async Task Login_InvalidEmail_ReturnsUnauthorized()
    {
        var dto = new UserLoginDto
        {
            Email = "nonexistent@example.com",
            Password = "password123"
        };

        var result = await _controller.Login(dto);

        Assert.That(result, Is.InstanceOf<UnauthorizedObjectResult>());
        var unauthorizedResult = result as UnauthorizedObjectResult;
        Assert.That(unauthorizedResult?.Value, Is.EqualTo("Invalid email or password."));
    }

    [Test]
    public async Task Login_InvalidPassword_ReturnsUnauthorized()
    {
        var user = new User
        {
            FullName = "Test User",
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("correctpassword"),
            Role = UserRole.Customer
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var dto = new UserLoginDto
        {
            Email = "test@example.com",
            Password = "wrongpassword"
        };

        var result = await _controller.Login(dto);

        Assert.That(result, Is.InstanceOf<UnauthorizedObjectResult>());
        var unauthorizedResult = result as UnauthorizedObjectResult;
        Assert.That(unauthorizedResult?.Value, Is.EqualTo("Invalid email or password."));
    }
}