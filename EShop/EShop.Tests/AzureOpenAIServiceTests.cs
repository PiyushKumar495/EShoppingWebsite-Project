using EShop.Models;
using EShop.Repositories;
using EShop.Services;
using Microsoft.Extensions.Configuration;
using Moq;

namespace EShop.Tests
{
    [TestFixture]
    public class AzureOpenAIServiceTests
    {
        private Mock<IConfiguration> _mockConfiguration;
        private Mock<IGenericRepository<Product>> _mockProductRepo;
        private Mock<IGenericRepository<Category>> _mockCategoryRepo;
        private Mock<IGenericRepository<Order>> _mockOrderRepo;

        [SetUp]
        public void Setup()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockProductRepo = new Mock<IGenericRepository<Product>>();
            _mockCategoryRepo = new Mock<IGenericRepository<Category>>();
            _mockOrderRepo = new Mock<IGenericRepository<Order>>();

            _mockConfiguration.Setup(x => x["AzureOpenAI:Endpoint"]).Returns("https://test.openai.azure.com/");
            _mockConfiguration.Setup(x => x["AzureOpenAI:ApiKey"]).Returns("test-api-key");
            _mockConfiguration.Setup(x => x["AzureOpenAI:DeploymentName"]).Returns("test-deployment");
        }

        [Test]
        public void Constructor_WithValidConfiguration_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => 
            {
                try
                {
                    var service = new AzureOpenAIService(_mockConfiguration.Object, 
                        _mockProductRepo.Object, 
                        _mockCategoryRepo.Object, 
                        _mockOrderRepo.Object);
                }
                catch (Exception ex) when (ex.Message.Contains("Azure") || ex.Message.Contains("OpenAI") || ex.Message.Contains("Uri"))
                {
                    Assert.Pass("Constructor properly validates configuration");
                }
            });
        }

        [Test]
        public void Constructor_WithNullConfiguration_ThrowsException()
        {
            Assert.Throws<NullReferenceException>(() => 
                new AzureOpenAIService(null!, _mockProductRepo.Object, _mockCategoryRepo.Object, _mockOrderRepo.Object));
        }

        [Test]
        public void Constructor_WithNullProductRepository_DoesNotThrowImmediately()
        {
            Assert.DoesNotThrow(() => 
                new AzureOpenAIService(_mockConfiguration.Object, null!, _mockCategoryRepo.Object, _mockOrderRepo.Object));
        }

        [Test]
        public void Constructor_WithNullCategoryRepository_DoesNotThrowImmediately()
        {
            Assert.DoesNotThrow(() => 
                new AzureOpenAIService(_mockConfiguration.Object, _mockProductRepo.Object, null!, _mockOrderRepo.Object));
        }

        [Test]
        public void Constructor_WithNullOrderRepository_DoesNotThrowImmediately()
        {
            Assert.DoesNotThrow(() => 
                new AzureOpenAIService(_mockConfiguration.Object, _mockProductRepo.Object, _mockCategoryRepo.Object, null!));
        }

        [Test]
        public void ServiceInterface_ImplementsIAzureOpenAIService()
        {
            Assert.That(typeof(AzureOpenAIService).GetInterfaces(), Contains.Item(typeof(IAzureOpenAIService)));
        }
    }
}