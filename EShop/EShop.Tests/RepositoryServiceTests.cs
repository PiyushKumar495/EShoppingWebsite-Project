using EShop.Models;
using EShop.Repositories;
using EShop.Services;
using Moq;

namespace EShop.Tests
{
    [TestFixture]
    public class RepositoryServiceTests
    {
        private Mock<IGenericRepository<Product>> _mockProductRepo;
        private Mock<IGenericRepository<Category>> _mockCategoryRepo;
        private Mock<IGenericRepository<Order>> _mockOrderRepo;
        private Mock<IGenericRepository<User>> _mockUserRepo;
        private Mock<IGenericRepository<Payment>> _mockPaymentRepo;
        private Mock<IGenericRepository<Cart>> _mockCartRepo;
        private Mock<IGenericRepository<CartItem>> _mockCartItemRepo;
        private Mock<IGenericRepository<OrderItem>> _mockOrderItemRepo;
        private Mock<IRepositoryCollection> _mockRepoCollection;
        private RepositoryService _service;

        [SetUp]
        public void Setup()
        {
            _mockProductRepo = new Mock<IGenericRepository<Product>>();
            _mockCategoryRepo = new Mock<IGenericRepository<Category>>();
            _mockOrderRepo = new Mock<IGenericRepository<Order>>();
            _mockUserRepo = new Mock<IGenericRepository<User>>();
            _mockPaymentRepo = new Mock<IGenericRepository<Payment>>();
            _mockCartRepo = new Mock<IGenericRepository<Cart>>();
            _mockCartItemRepo = new Mock<IGenericRepository<CartItem>>();
            _mockOrderItemRepo = new Mock<IGenericRepository<OrderItem>>();

            _mockRepoCollection = new Mock<IRepositoryCollection>();
            _mockRepoCollection.Setup(x => x.ProductRepository).Returns(_mockProductRepo.Object);
            _mockRepoCollection.Setup(x => x.CategoryRepository).Returns(_mockCategoryRepo.Object);
            _mockRepoCollection.Setup(x => x.OrderRepository).Returns(_mockOrderRepo.Object);
            _mockRepoCollection.Setup(x => x.UserRepository).Returns(_mockUserRepo.Object);
            _mockRepoCollection.Setup(x => x.PaymentRepository).Returns(_mockPaymentRepo.Object);
            _mockRepoCollection.Setup(x => x.CartRepository).Returns(_mockCartRepo.Object);
            _mockRepoCollection.Setup(x => x.CartItemRepository).Returns(_mockCartItemRepo.Object);
            _mockRepoCollection.Setup(x => x.OrderItemRepository).Returns(_mockOrderItemRepo.Object);

            _service = new RepositoryService(_mockRepoCollection.Object);
        }

        [Test]
        public void Constructor_InitializesAllRepositories()
        {
            Assert.Multiple(() =>
            {
                Assert.That(_service.ProductRepository, Is.EqualTo(_mockProductRepo.Object));
                Assert.That(_service.CategoryRepository, Is.EqualTo(_mockCategoryRepo.Object));
                Assert.That(_service.OrderRepository, Is.EqualTo(_mockOrderRepo.Object));
                Assert.That(_service.UserRepository, Is.EqualTo(_mockUserRepo.Object));
                Assert.That(_service.PaymentRepository, Is.EqualTo(_mockPaymentRepo.Object));
                Assert.That(_service.CartRepository, Is.EqualTo(_mockCartRepo.Object));
                Assert.That(_service.CartItemRepository, Is.EqualTo(_mockCartItemRepo.Object));
                Assert.That(_service.OrderItemRepository, Is.EqualTo(_mockOrderItemRepo.Object));
            });
        }

        [Test]
        public void RepositoryCollection_Constructor_InitializesAllRepositories()
        {
            var repoCollection = new RepositoryCollection(
                _mockProductRepo.Object,
                _mockCategoryRepo.Object,
                _mockOrderRepo.Object,
                _mockUserRepo.Object,
                _mockPaymentRepo.Object,
                _mockCartRepo.Object,
                _mockCartItemRepo.Object,
                _mockOrderItemRepo.Object
            );

            Assert.Multiple(() =>
            {
                Assert.That(repoCollection.ProductRepository, Is.EqualTo(_mockProductRepo.Object));
                Assert.That(repoCollection.CategoryRepository, Is.EqualTo(_mockCategoryRepo.Object));
                Assert.That(repoCollection.OrderRepository, Is.EqualTo(_mockOrderRepo.Object));
                Assert.That(repoCollection.UserRepository, Is.EqualTo(_mockUserRepo.Object));
                Assert.That(repoCollection.PaymentRepository, Is.EqualTo(_mockPaymentRepo.Object));
                Assert.That(repoCollection.CartRepository, Is.EqualTo(_mockCartRepo.Object));
                Assert.That(repoCollection.CartItemRepository, Is.EqualTo(_mockCartItemRepo.Object));
                Assert.That(repoCollection.OrderItemRepository, Is.EqualTo(_mockOrderItemRepo.Object));
            });
        }
    }
}