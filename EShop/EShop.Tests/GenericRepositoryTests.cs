using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using EShop.Data;
using EShop.Repositories;
using EShop.Models;
using System.Threading.Tasks;
using System.Linq;

namespace EShop.Tests
{
    [TestFixture]
    public class GenericRepositoryTests
    {
        private EshoppingDbContext _context;
        private GenericRepository<Product> _repository;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<EshoppingDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
            _context = new EshoppingDbContext(options);
            _repository = new GenericRepository<Product>(_context);
        }

        [Test]
        public async Task GetAllAsync_ReturnsAllEntities()
        {
            var product = new Product { Name = "Test", Description = "Test Description", Price = 100, StockQuantity = 10 };
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            var result = await _repository.GetAllAsync();
            Assert.That(result.Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task GetByIdAsync_ReturnsEntity_WhenExists()
        {
            var product = new Product { Name = "Test", Description = "Test Description", Price = 100, StockQuantity = 10 };
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            var result = await _repository.GetByIdAsync(product.ProductId);
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Name, Is.EqualTo("Test"));
        }

        [Test]
        public async Task GetByIdAsync_ReturnsNull_WhenNotExists()
        {
            var result = await _repository.GetByIdAsync(999);
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task AddAsync_AddsEntity()
        {
            var product = new Product { Name = "Test", Description = "Test Description", Price = 100, StockQuantity = 10 };
            await _repository.AddAsync(product);

            var result = await _context.Products.FindAsync(product.ProductId);
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task UpdateAsync_UpdatesEntity()
        {
            var product = new Product { Name = "Test", Description = "Test Description", Price = 100, StockQuantity = 10 };
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            product.Name = "Updated";
            await _repository.UpdateAsync(product);

            var result = await _context.Products.FindAsync(product.ProductId);
            Assert.That(result!.Name, Is.EqualTo("Updated"));
        }

        [Test]
        public async Task DeleteAsync_DeletesEntity_WhenExists()
        {
            var product = new Product { Name = "Test", Description = "Test Description", Price = 100, StockQuantity = 10 };
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            await _repository.DeleteAsync(product.ProductId);

            var result = await _context.Products.FindAsync(product.ProductId);
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task DeleteAsync_DoesNothing_WhenNotExists()
        {
            await _repository.DeleteAsync(999);
            // Should not throw exception
            Assert.Pass();
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }
    }
}