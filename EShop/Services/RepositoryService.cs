using EShop.Models;
using EShop.Repositories;

namespace EShop.Services
{
    public class RepositoryService
    {
        public IGenericRepository<Product> ProductRepository { get; }
        public IGenericRepository<Category> CategoryRepository { get; }
        public IGenericRepository<Order> OrderRepository { get; }
        public IGenericRepository<User> UserRepository { get; }
        public IGenericRepository<Payment> PaymentRepository { get; }
        public IGenericRepository<Cart> CartRepository { get; }
        public IGenericRepository<CartItem> CartItemRepository { get; }
        public IGenericRepository<OrderItem> OrderItemRepository { get; }

        public RepositoryService(IRepositoryCollection repositories)
        {
            ProductRepository = repositories.ProductRepository;
            CategoryRepository = repositories.CategoryRepository;
            OrderRepository = repositories.OrderRepository;
            UserRepository = repositories.UserRepository;
            PaymentRepository = repositories.PaymentRepository;
            CartRepository = repositories.CartRepository;
            CartItemRepository = repositories.CartItemRepository;
            OrderItemRepository = repositories.OrderItemRepository;
        }
    }

    public interface IRepositoryCollection
    {
        IGenericRepository<Product> ProductRepository { get; }
        IGenericRepository<Category> CategoryRepository { get; }
        IGenericRepository<Order> OrderRepository { get; }
        IGenericRepository<User> UserRepository { get; }
        IGenericRepository<Payment> PaymentRepository { get; }
        IGenericRepository<Cart> CartRepository { get; }
        IGenericRepository<CartItem> CartItemRepository { get; }
        IGenericRepository<OrderItem> OrderItemRepository { get; }
    }

    public class RepositoryCollection : IRepositoryCollection
    {
        public IGenericRepository<Product> ProductRepository { get; }
        public IGenericRepository<Category> CategoryRepository { get; }
        public IGenericRepository<Order> OrderRepository { get; }
        public IGenericRepository<User> UserRepository { get; }
        public IGenericRepository<Payment> PaymentRepository { get; }
        public IGenericRepository<Cart> CartRepository { get; }
        public IGenericRepository<CartItem> CartItemRepository { get; }
        public IGenericRepository<OrderItem> OrderItemRepository { get; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S107:Methods should not have too many parameters", Justification = "DI constructor pattern")]
        public RepositoryCollection(
            IGenericRepository<Product> productRepository,
            IGenericRepository<Category> categoryRepository,
            IGenericRepository<Order> orderRepository,
            IGenericRepository<User> userRepository,
            IGenericRepository<Payment> paymentRepository,
            IGenericRepository<Cart> cartRepository,
            IGenericRepository<CartItem> cartItemRepository,
            IGenericRepository<OrderItem> orderItemRepository)
        {
            ProductRepository = productRepository;
            CategoryRepository = categoryRepository;
            OrderRepository = orderRepository;
            UserRepository = userRepository;
            PaymentRepository = paymentRepository;
            CartRepository = cartRepository;
            CartItemRepository = cartItemRepository;
            OrderItemRepository = orderItemRepository;
        }
    }
}