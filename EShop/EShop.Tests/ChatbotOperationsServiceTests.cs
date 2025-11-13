using EShop.Models;
using EShop.Repositories;
using EShop.Services;
using Moq;

namespace EShop.Tests
{
    [TestFixture]
    public class ChatbotOperationsServiceTests
    {
        private Mock<IGenericRepository<Product>> _mockProductRepo;
        private Mock<IGenericRepository<Category>> _mockCategoryRepo;
        private Mock<IGenericRepository<Order>> _mockOrderRepo;
        private Mock<IGenericRepository<User>> _mockUserRepo;
        private Mock<IGenericRepository<Payment>> _mockPaymentRepo;
        private Mock<IGenericRepository<Cart>> _mockCartRepo;
        private Mock<IGenericRepository<CartItem>> _mockCartItemRepo;
        private Mock<IGenericRepository<OrderItem>> _mockOrderItemRepo;
        private RepositoryService _repositoryService;
        private ChatbotOperationsService _service;

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

            var mockRepoCollection = new Mock<IRepositoryCollection>();
            mockRepoCollection.Setup(x => x.ProductRepository).Returns(_mockProductRepo.Object);
            mockRepoCollection.Setup(x => x.CategoryRepository).Returns(_mockCategoryRepo.Object);
            mockRepoCollection.Setup(x => x.OrderRepository).Returns(_mockOrderRepo.Object);
            mockRepoCollection.Setup(x => x.UserRepository).Returns(_mockUserRepo.Object);
            mockRepoCollection.Setup(x => x.PaymentRepository).Returns(_mockPaymentRepo.Object);
            mockRepoCollection.Setup(x => x.CartRepository).Returns(_mockCartRepo.Object);
            mockRepoCollection.Setup(x => x.CartItemRepository).Returns(_mockCartItemRepo.Object);
            mockRepoCollection.Setup(x => x.OrderItemRepository).Returns(_mockOrderItemRepo.Object);

            _repositoryService = new RepositoryService(mockRepoCollection.Object);
            _service = new ChatbotOperationsService(_repositoryService);
        }

        [Test]
        public async Task ExecuteAdminOperationAsync_NonAdminUser_ReturnsAccessDenied()
        {
            var result = await _service.ExecuteAdminOperationAsync("add product test", "Customer");
            Assert.That(result, Is.EqualTo("❌ Admin access required for this operation."));
        }

        [Test]
        public async Task ExecuteAdminOperationAsync_AddProduct_Success()
        {
            var categories = new List<Category> { new() { CategoryId = 1, CategoryName = "Electronics" } };
            _mockCategoryRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(categories);
            _mockProductRepo.Setup(x => x.AddAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);

            var result = await _service.ExecuteAdminOperationAsync("add product iPhone price 50000 stock 10 category Electronics", "Admin");

            Assert.That(result, Does.Contain("✅ Product 'iPhone' added successfully!"));
        }

        [Test]
        public async Task ExecuteAdminOperationAsync_UpdateProductPrice_Success()
        {
            var product = new Product { ProductId = 1, Name = "iPhone", Price = 40000 };
            _mockProductRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(product);
            _mockProductRepo.Setup(x => x.UpdateAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);

            var result = await _service.ExecuteAdminOperationAsync("update product 1 price 50000", "Admin");

            Assert.Multiple(() =>
            {
                Assert.That(result, Does.Contain("✅ Product 'iPhone' price updated to ₹50000"));
                Assert.That(product.Price, Is.EqualTo(50000));
            });
        }

        [Test]
        public async Task ExecuteAdminOperationAsync_UpdateProductStock_Success()
        {
            var product = new Product { ProductId = 1, Name = "iPhone", StockQuantity = 10 };
            _mockProductRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(product);
            _mockProductRepo.Setup(x => x.UpdateAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);

            var result = await _service.ExecuteAdminOperationAsync("update product 1 stock 20", "Admin");

            Assert.That(result, Does.Contain("✅ Product 'iPhone' stock updated to 20"));
        }

        [Test]
        public async Task ExecuteAdminOperationAsync_DeleteProduct_Success()
        {
            var product = new Product { ProductId = 1, Name = "iPhone" };
            _mockProductRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(product);
            _mockProductRepo.Setup(x => x.DeleteAsync(1)).Returns(Task.CompletedTask);

            var result = await _service.ExecuteAdminOperationAsync("delete product 1", "Admin");

            Assert.That(result, Does.Contain("✅ Product 'iPhone' deleted successfully!"));
        }

        [Test]
        public async Task ExecuteAdminOperationAsync_LowStock_ReturnsLowStockProducts()
        {
            var products = new List<Product>
            {
                new() { ProductId = 1, Name = "iPhone", StockQuantity = 5 },
                new() { ProductId = 2, Name = "Samsung", StockQuantity = 15 }
            };
            _mockProductRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(products);

            var result = await _service.ExecuteAdminOperationAsync("low stock", "Admin");

            Assert.That(result, Does.Contain("⚠️ Low Stock Products:"));
        }

        [Test]
        public async Task ExecuteAdminOperationAsync_AddCategory_Success()
        {
            _mockCategoryRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<Category>());
            _mockCategoryRepo.Setup(x => x.AddAsync(It.IsAny<Category>())).Returns(Task.CompletedTask);

            var result = await _service.ExecuteAdminOperationAsync("add category Electronics", "Admin");

            Assert.That(result, Does.Contain("✅ Category 'Electronics' created successfully!"));
        }

        [Test]
        public async Task ExecuteAdminOperationAsync_UpdateOrderStatus_Success()
        {
            var order = new Order { OrderId = 1, Status = OrderStatus.Pending };
            _mockOrderRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(order);
            _mockOrderRepo.Setup(x => x.UpdateAsync(It.IsAny<Order>())).Returns(Task.CompletedTask);

            var result = await _service.ExecuteAdminOperationAsync("update order 1 status Shipped", "Admin");

            Assert.That(result, Does.Contain("✅ Order #1 status updated to Shipped"));
        }

        [Test]
        public async Task ExecuteAdminOperationAsync_SalesReport_Success()
        {
            var payments = new List<Payment>
            {
                new() { PaymentId = 1, Amount = 50000, Status = PaymentStatus.Completed }
            };
            _mockOrderRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<Order>());
            _mockPaymentRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(payments);

            var result = await _service.ExecuteAdminOperationAsync("sales report", "Admin");

            Assert.That(result, Does.Contain("💰 Sales Report:"));
        }

        [Test]
        public async Task ExecuteAdminOperationAsync_UserStatistics_Success()
        {
            var users = new List<User>
            {
                new() { UserId = 1, Role = UserRole.Admin },
                new() { UserId = 2, Role = UserRole.Customer }
            };
            _mockUserRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(users);
            _mockOrderRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<Order>());

            var result = await _service.ExecuteAdminOperationAsync("user statistics", "Admin");

            Assert.That(result, Does.Contain("👥 User Statistics:"));
        }

        [Test]
        public async Task ExecuteAdminOperationAsync_InventoryReport_Success()
        {
            var products = new List<Product>
            {
                new() { ProductId = 1, Price = 50000, StockQuantity = 10 }
            };
            _mockProductRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(products);

            var result = await _service.ExecuteAdminOperationAsync("inventory report", "Admin");

            Assert.That(result, Does.Contain("📦 Inventory Report:"));
        }

        [Test]
        public async Task ExecuteCustomerOperationAsync_ProductSearch_ReturnsProducts()
        {
            var products = new List<Product>
            {
                new() { ProductId = 1, Name = "iPhone 14", Description = "Latest iPhone", Price = 50000, StockQuantity = 10 }
            };
            _mockProductRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(products);

            var result = await _service.ExecuteCustomerOperationAsync("search iPhone", 1);

            Assert.That(result, Does.Contain("🛍️ Found Products:"));
        }

        [Test]
        public async Task ExecuteCustomerOperationAsync_AddToCart_Success()
        {
            var products = new List<Product>
            {
                new() { ProductId = 1, Name = "iPhone", Price = 50000, StockQuantity = 10 }
            };
            var carts = new List<Cart> { new() { CartId = 1, UserId = 1 } };
            var cartItems = new List<CartItem>();

            _mockProductRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(products);
            _mockCartRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(carts);
            _mockCartItemRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(cartItems);
            _mockCartItemRepo.Setup(x => x.AddAsync(It.IsAny<CartItem>())).Returns(Task.CompletedTask);

            var result = await _service.ExecuteCustomerOperationAsync("add iPhone to cart", 1);

            Assert.That(result, Does.Contain("✅ Added 1x 'iPhone' to your cart!"));
        }

        [Test]
        public async Task ExecuteCustomerOperationAsync_ViewCart_WithItems()
        {
            var carts = new List<Cart> { new() { CartId = 1, UserId = 1 } };
            var cartItems = new List<CartItem>
            {
                new() { CartItemId = 1, CartId = 1, ProductId = 1, Quantity = 2, TotalPrice = 100000 }
            };
            var products = new List<Product>
            {
                new() { ProductId = 1, Name = "iPhone", Price = 50000 }
            };

            _mockCartRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(carts);
            _mockCartItemRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(cartItems);
            _mockProductRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(products);

            var result = await _service.ExecuteCustomerOperationAsync("view cart", 1);

            Assert.That(result, Does.Contain("🛒 Your Cart:"));
        }

        [Test]
        public async Task ExecuteCustomerOperationAsync_CheckoutSuccess()
        {
            var carts = new List<Cart> { new() { CartId = 1, UserId = 1 } };
            var cartItems = new List<CartItem>
            {
                new() { CartItemId = 1, CartId = 1, ProductId = 1, Quantity = 2, TotalPrice = 100000 }
            };
            var products = new List<Product>
            {
                new() { ProductId = 1, Name = "iPhone", Price = 50000, StockQuantity = 10 }
            };

            _mockCartRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(carts);
            _mockCartItemRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(cartItems);
            _mockProductRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(products);
            _mockOrderRepo.Setup(x => x.AddAsync(It.IsAny<Order>())).Returns(Task.CompletedTask);
            _mockOrderItemRepo.Setup(x => x.AddAsync(It.IsAny<OrderItem>())).Returns(Task.CompletedTask);
            _mockPaymentRepo.Setup(x => x.AddAsync(It.IsAny<Payment>())).Returns(Task.CompletedTask);
            _mockProductRepo.Setup(x => x.UpdateAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);
            _mockCartItemRepo.Setup(x => x.DeleteAsync(It.IsAny<int>())).Returns(Task.CompletedTask);

            var result = await _service.ExecuteCustomerOperationAsync("checkout, address Delhi, payment mode COD", 1);

            Assert.That(result, Does.Contain("✅ Order placed successfully from your cart!"));
        }

        [Test]
        public async Task ExecuteCustomerOperationAsync_PlaceOrder_Success()
        {
            var products = new List<Product>
            {
                new() { ProductId = 1, Name = "iPhone", Price = 50000, StockQuantity = 10 }
            };
            _mockProductRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(products);
            _mockOrderRepo.Setup(x => x.AddAsync(It.IsAny<Order>())).Returns(Task.CompletedTask);
            _mockOrderItemRepo.Setup(x => x.AddAsync(It.IsAny<OrderItem>())).Returns(Task.CompletedTask);
            _mockPaymentRepo.Setup(x => x.AddAsync(It.IsAny<Payment>())).Returns(Task.CompletedTask);
            _mockProductRepo.Setup(x => x.UpdateAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);

            var result = await _service.ExecuteCustomerOperationAsync("order 1 iPhone, address Delhi, payment mode COD", 1);

            Assert.That(result, Does.Contain("✅ Order placed successfully!"));
        }

        [Test]
        public async Task ExecuteCustomerOperationAsync_OrderStatus_Success()
        {
            var order = new Order
            {
                OrderId = 1,
                UserId = 1,
                Status = OrderStatus.Shipped,
                TotalAmount = 50000,
                OrderDate = DateTime.Today,
                PaymentMethod = PaymentMethod.COD
            };
            _mockOrderRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(order);

            var result = await _service.ExecuteCustomerOperationAsync("check order 1", 1);

            Assert.That(result, Does.Contain("📦 Order #1:"));
        }

        [Test]
        public async Task ExecuteCustomerOperationAsync_CancelOrder_Success()
        {
            var order = new Order { OrderId = 1, UserId = 1, Status = OrderStatus.Pending };
            var orderItems = new List<OrderItem> { new() { OrderId = 1, ProductId = 1, Quantity = 2 } };
            var product = new Product { ProductId = 1, StockQuantity = 5 };

            _mockOrderRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(order);
            _mockOrderItemRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(orderItems);
            _mockProductRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(product);
            _mockOrderRepo.Setup(x => x.UpdateAsync(It.IsAny<Order>())).Returns(Task.CompletedTask);
            _mockProductRepo.Setup(x => x.UpdateAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);

            var result = await _service.ExecuteCustomerOperationAsync("cancel order 1", 1);

            Assert.That(result, Does.Contain("✅ Order #1 has been cancelled successfully"));
        }

        [Test]
        public async Task ExecuteCustomerOperationAsync_MultiProductOrder_Success()
        {
            var products = new List<Product>
            {
                new() { ProductId = 1, Name = "iPhone", Price = 50000, StockQuantity = 10 },
                new() { ProductId = 2, Name = "Samsung", Price = 40000, StockQuantity = 5 }
            };
            _mockProductRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(products);
            _mockOrderRepo.Setup(x => x.AddAsync(It.IsAny<Order>())).Returns(Task.CompletedTask);
            _mockOrderItemRepo.Setup(x => x.AddAsync(It.IsAny<OrderItem>())).Returns(Task.CompletedTask);
            _mockPaymentRepo.Setup(x => x.AddAsync(It.IsAny<Payment>())).Returns(Task.CompletedTask);
            _mockProductRepo.Setup(x => x.UpdateAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);

            var result = await _service.ExecuteCustomerOperationAsync("multi order 1 iPhone, 2 Samsung, address Delhi, payment mode ONLINE", 1);

            Assert.That(result, Does.Contain("✅ Multi-product order placed successfully!"));
        }

        [Test]
        public async Task ExecuteCustomerOperationAsync_FlexibleOrderStatus_Success()
        {
            var order = new Order
            {
                OrderId = 123,
                UserId = 1,
                Status = OrderStatus.Shipped,
                TotalAmount = 50000,
                OrderDate = DateTime.Today,
                PaymentMethod = PaymentMethod.COD
            };
            _mockOrderRepo.Setup(x => x.GetByIdAsync(123)).ReturnsAsync(order);

            var result = await _service.ExecuteCustomerOperationAsync("check my order 123 status", 1);

            Assert.That(result, Does.Contain("📦 Order #123:"));
        }

        [Test]
        public async Task ExecuteCustomerOperationAsync_FlexibleCheckout_Success()
        {
            var carts = new List<Cart> { new() { CartId = 1, UserId = 1 } };
            var cartItems = new List<CartItem>
            {
                new() { CartItemId = 1, CartId = 1, ProductId = 1, Quantity = 1, TotalPrice = 50000 }
            };
            var products = new List<Product>
            {
                new() { ProductId = 1, Name = "iPhone", Price = 50000, StockQuantity = 10 }
            };

            _mockCartRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(carts);
            _mockCartItemRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(cartItems);
            _mockProductRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(products);
            _mockOrderRepo.Setup(x => x.AddAsync(It.IsAny<Order>())).Returns(Task.CompletedTask);
            _mockOrderItemRepo.Setup(x => x.AddAsync(It.IsAny<OrderItem>())).Returns(Task.CompletedTask);
            _mockPaymentRepo.Setup(x => x.AddAsync(It.IsAny<Payment>())).Returns(Task.CompletedTask);
            _mockProductRepo.Setup(x => x.UpdateAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);
            _mockCartItemRepo.Setup(x => x.DeleteAsync(It.IsAny<int>())).Returns(Task.CompletedTask);

            var result = await _service.ExecuteCustomerOperationAsync("checkout with address Mumbai and online payment", 1);

            Assert.That(result, Does.Contain("✅ Order placed successfully from your cart!"));
        }

        [Test]
        public async Task ExecuteCustomerOperationAsync_ClearCart_Success()
        {
            var carts = new List<Cart> { new() { CartId = 1, UserId = 1 } };
            var cartItems = new List<CartItem> { new() { CartItemId = 1, CartId = 1 } };

            _mockCartRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(carts);
            _mockCartItemRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(cartItems);
            _mockCartItemRepo.Setup(x => x.DeleteAsync(1)).Returns(Task.CompletedTask);

            var result = await _service.ExecuteCustomerOperationAsync("clear cart", 1);

            Assert.That(result, Is.EqualTo("✅ Your cart has been cleared successfully!"));
        }

        [Test]
        public async Task ExecuteAdminOperationAsync_CommaDelimitedUpdate_Success()
        {
            var product = new Product { ProductId = 1, Name = "iPhone", Price = 40000 };
            _mockProductRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(product);
            _mockProductRepo.Setup(x => x.UpdateAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);

            var result = await _service.ExecuteAdminOperationAsync("1,50000", "Admin");

            Assert.That(result, Does.Contain("✅ Product 'iPhone' price updated to ₹50000"));
        }

        [Test]
        public async Task ExecuteAdminOperationAsync_DeleteProductByName_Success()
        {
            var products = new List<Product> { new() { ProductId = 1, Name = "iPhone" } };
            _mockProductRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(products);
            _mockProductRepo.Setup(x => x.DeleteAsync(1)).Returns(Task.CompletedTask);

            var result = await _service.ExecuteAdminOperationAsync("delete iPhone", "Admin");

            Assert.That(result, Does.Contain("✅ Product 'iPhone' deleted successfully!"));
        }

        [Test]
        public async Task ExecuteCustomerOperationAsync_AddToCartByProductName_Success()
        {
            var product = new Product { ProductId = 5, Name = "Samsung", Price = 40000, StockQuantity = 10 };
            var products = new List<Product> { product };
            var carts = new List<Cart> { new() { CartId = 1, UserId = 1 } };
            var cartItems = new List<CartItem>();

            _mockProductRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(products);
            _mockCartRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(carts);
            _mockCartItemRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(cartItems);
            _mockCartItemRepo.Setup(x => x.AddAsync(It.IsAny<CartItem>())).Returns(Task.CompletedTask);

            var result = await _service.ExecuteCustomerOperationAsync("add Samsung to cart", 1);

            Assert.That(result, Does.Contain("Samsung"));
        }

        [Test]
        public async Task ExecuteCustomerOperationAsync_FlexibleAddToCart_Success()
        {
            var products = new List<Product> { new() { ProductId = 1, Name = "iPhone", Price = 50000, StockQuantity = 10 } };
            var carts = new List<Cart> { new() { CartId = 1, UserId = 1 } };
            var cartItems = new List<CartItem>();

            _mockProductRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(products);
            _mockCartRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(carts);
            _mockCartItemRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(cartItems);
            _mockCartItemRepo.Setup(x => x.AddAsync(It.IsAny<CartItem>())).Returns(Task.CompletedTask);

            var result = await _service.ExecuteCustomerOperationAsync("add 2 iPhone to my cart", 1);

            Assert.That(result, Does.Contain("iPhone"));
        }

        [Test]
        public async Task ExecuteCustomerOperationAsync_FlexiblePlaceOrder_Success()
        {
            var products = new List<Product> { new() { ProductId = 1, Name = "iPhone", Price = 50000, StockQuantity = 10 } };
            _mockProductRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(products);
            _mockOrderRepo.Setup(x => x.AddAsync(It.IsAny<Order>())).Returns(Task.CompletedTask);
            _mockOrderItemRepo.Setup(x => x.AddAsync(It.IsAny<OrderItem>())).Returns(Task.CompletedTask);
            _mockPaymentRepo.Setup(x => x.AddAsync(It.IsAny<Payment>())).Returns(Task.CompletedTask);
            _mockProductRepo.Setup(x => x.UpdateAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);

            var result = await _service.ExecuteCustomerOperationAsync("I want to order 2 iPhone, deliver to Bangalore, payment online", 1);

            Assert.That(result, Does.Contain("✅"));
        }


        // Error and Edge Case Tests
        [Test]
        public async Task ExecuteAdminOperationAsync_AddProduct_CategoryNotFound()
        {
            _mockCategoryRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<Category>());

            var result = await _service.ExecuteAdminOperationAsync("add product iPhone price 50000 stock 10 category Electronics", "Admin");

            Assert.That(result, Does.Contain("❌ Category 'Electronics' not found"));
        }

        [Test]
        public async Task ExecuteAdminOperationAsync_UpdateNonExistentProduct_ReturnsError()
        {
            _mockProductRepo.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((Product?)null);

            var result = await _service.ExecuteAdminOperationAsync("update product 999 price 50000", "Admin");

            Assert.That(result, Does.Contain("❌ Product with ID 999 not found"));
        }

        [Test]
        public async Task ExecuteAdminOperationAsync_DeleteNonExistentProduct_ReturnsError()
        {
            _mockProductRepo.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((Product?)null);

            var result = await _service.ExecuteAdminOperationAsync("delete product 999", "Admin");

            Assert.That(result, Does.Contain("❌ Product with ID 999 not found"));
        }

        [Test]
        public async Task ExecuteAdminOperationAsync_AddDuplicateCategory_ReturnsError()
        {
            var categories = new List<Category> { new() { CategoryId = 1, CategoryName = "Electronics" } };
            _mockCategoryRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(categories);

            var result = await _service.ExecuteAdminOperationAsync("add category Electronics", "Admin");

            Assert.That(result, Does.Contain("❌ Category 'Electronics' already exists"));
        }

        [Test]
        public async Task ExecuteAdminOperationAsync_DeleteCategoryWithProducts_ReturnsError()
        {
            var category = new Category { CategoryId = 1, CategoryName = "Electronics" };
            var products = new List<Product> { new() { ProductId = 1, CategoryId = 1, Name = "Phone" } };

            _mockCategoryRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(category);
            _mockProductRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(products);

            var result = await _service.ExecuteAdminOperationAsync("delete category 1", "Admin");

            Assert.That(result, Does.Contain("❌ Cannot delete category 'Electronics' - it contains products"));
        }

        [Test]
        public async Task ExecuteAdminOperationAsync_UpdateOrderInvalidStatus_ReturnsError()
        {
            var result = await _service.ExecuteAdminOperationAsync("update order 1 status InvalidStatus", "Admin");

            Assert.That(result, Does.Contain("❌ Invalid status. Use: Pending, Shipped, Delivered, Cancelled"));
        }

        [Test]
        public async Task ExecuteCustomerOperationAsync_AddToCartWithoutLogin_ReturnsError()
        {
            var result = await _service.ExecuteCustomerOperationAsync("add iPhone to cart", null);

            Assert.That(result, Is.EqualTo("❌ Please login to add items to your cart."));
        }

        [Test]
        public async Task ExecuteCustomerOperationAsync_AddToCartInsufficientStock_ReturnsError()
        {
            var products = new List<Product>
            {
                new() { ProductId = 1, Name = "iPhone", Price = 50000, StockQuantity = 2 }
            };
            _mockProductRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(products);

            var result = await _service.ExecuteCustomerOperationAsync("add 5 iPhone to cart", 1);

            Assert.That(result, Does.Contain("❌ Only 2 units of 'iPhone' available in stock"));
        }

        [Test]
        public async Task ExecuteCustomerOperationAsync_AddToCartProductNotFound_ReturnsError()
        {
            _mockProductRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<Product>());

            var result = await _service.ExecuteCustomerOperationAsync("add NonExistentProduct to cart", 1);

            Assert.That(result, Does.Contain("❌ No products found in your request"));
        }

        [Test]
        public async Task ExecuteCustomerOperationAsync_CheckoutEmptyCart_ReturnsError()
        {
            _mockCartRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<Cart>());

            var result = await _service.ExecuteCustomerOperationAsync("checkout, address Delhi, payment mode COD", 1);

            Assert.That(result, Does.Contain("❌ Your cart is empty"));
        }

        [Test]
        public async Task ExecuteCustomerOperationAsync_CheckoutInsufficientStock_ReturnsError()
        {
            var carts = new List<Cart> { new() { CartId = 1, UserId = 1 } };
            var cartItems = new List<CartItem>
            {
                new() { CartItemId = 1, CartId = 1, ProductId = 1, Quantity = 5, TotalPrice = 250000 }
            };
            var products = new List<Product>
            {
                new() { ProductId = 1, Name = "iPhone", Price = 50000, StockQuantity = 2 }
            };

            _mockCartRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(carts);
            _mockCartItemRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(cartItems);
            _mockProductRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(products);

            var result = await _service.ExecuteCustomerOperationAsync("checkout, address Delhi, payment mode COD", 1);

            Assert.That(result, Does.Contain("❌ Insufficient stock for 'iPhone'"));
        }

        [Test]
        public async Task ExecuteCustomerOperationAsync_OrderStatusNotFound_ReturnsError()
        {
            _mockOrderRepo.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((Order?)null);

            var result = await _service.ExecuteCustomerOperationAsync("check order 999", 1);

            Assert.That(result, Does.Contain("❌ Order #999 not found"));
        }

        [Test]
        public async Task ExecuteCustomerOperationAsync_OrderStatus_WrongUser()
        {
            var order = new Order { OrderId = 1, UserId = 2 };
            _mockOrderRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(order);

            var result = await _service.ExecuteCustomerOperationAsync("check order 1", 1);

            Assert.That(result, Is.EqualTo("❌ This order doesn't belong to your account."));
        }

        [Test]
        public async Task ExecuteCustomerOperationAsync_CancelAlreadyCancelledOrder_ReturnsInfo()
        {
            var order = new Order { OrderId = 1, UserId = 1, Status = OrderStatus.Cancelled };
            _mockOrderRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(order);

            var result = await _service.ExecuteCustomerOperationAsync("cancel order 1", 1);

            Assert.That(result, Does.Contain("ℹ️ Order #1 is already cancelled"));
        }

        [Test]
        public async Task ExecuteCustomerOperationAsync_CancelOrder_AlreadyDelivered()
        {
            var order = new Order { OrderId = 1, UserId = 1, Status = OrderStatus.Delivered };
            _mockOrderRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(order);

            var result = await _service.ExecuteCustomerOperationAsync("cancel order 1", 1);

            Assert.That(result, Is.EqualTo("❌ Order #1 has already been delivered and cannot be cancelled."));
        }

        [Test]
        public async Task ExecuteCustomerOperationAsync_ViewCart_EmptyCart()
        {
            _mockCartRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<Cart>());

            var result = await _service.ExecuteCustomerOperationAsync("view cart", 1);

            Assert.That(result, Is.EqualTo("🛒 Your cart is empty. Start shopping to add items!"));
        }

        [Test]
        public async Task ExecuteCustomerOperationAsync_SearchNoResults_ReturnsError()
        {
            _mockProductRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<Product>());

            var result = await _service.ExecuteCustomerOperationAsync("search nonexistent", 1);

            Assert.That(result, Is.EqualTo("❌ No products found matching your search."));
        }

        [Test]
        public async Task ExecuteAdminOperationAsync_UnknownOperation_ReturnsHelp()
        {
            var result = await _service.ExecuteAdminOperationAsync("unknown operation", "Admin");

            Assert.That(result, Does.Contain("ℹ️ Available admin operations:"));
        }

        [Test]
        public async Task ExecuteCustomerOperationAsync_UnknownOperation_ReturnsHelp()
        {
            var result = await _service.ExecuteCustomerOperationAsync("unknown operation", 1);

            Assert.That(result, Does.Contain("ℹ️ I can help you"));
        }

        [Test]
        public async Task ExecuteAdminOperationAsync_EmptyCommand_ReturnsHelp()
        {
            var result = await _service.ExecuteAdminOperationAsync("", "Admin");

            Assert.That(result, Does.Contain("ℹ️ Available admin operations:"));
        }

        [Test]
        public async Task ExecuteCustomerOperationAsync_EmptyCommand_ReturnsHelp()
        {
            var result = await _service.ExecuteCustomerOperationAsync("", 1);

            Assert.That(result, Does.Contain("ℹ️ I can help you"));
        }

        [Test]
        public async Task ExecuteAdminOperationAsync_LowStockNoLowStockProducts_ReturnsSuccess()
        {
            var products = new List<Product>
            {
                new() { ProductId = 1, Name = "iPhone", StockQuantity = 50 },
                new() { ProductId = 2, Name = "Samsung", StockQuantity = 25 }
            };
            _mockProductRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(products);

            var result = await _service.ExecuteAdminOperationAsync("low stock", "Admin");

            Assert.That(result, Is.EqualTo("✅ All products have sufficient stock!"));
        }

        [Test]
        public async Task ExecuteAdminOperationAsync_OrderStatistics_ReturnsStats()
        {
            var orders = new List<Order>
            {
                new() { OrderId = 1, Status = OrderStatus.Pending, OrderDate = DateTime.Today },
                new() { OrderId = 2, Status = OrderStatus.Shipped, OrderDate = DateTime.Today.AddDays(-1) }
            };
            _mockOrderRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(orders);

            var result = await _service.ExecuteAdminOperationAsync("order statistics", "Admin");

            Assert.That(result, Does.Contain("📊 Order Statistics:"));
        }

        [Test]
        public async Task ExecuteAdminOperationAsync_ListCategories_Success()
        {
            var categories = new List<Category>
            {
                new() { CategoryId = 1, CategoryName = "Electronics" },
                new() { CategoryId = 2, CategoryName = "Clothing" }
            };
            _mockCategoryRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(categories);

            var result = await _service.ExecuteAdminOperationAsync("list categories", "Admin");

            Assert.That(result, Does.Contain("📂 Categories:"));
        }

        [Test]
        public async Task ExecuteAdminOperationAsync_PendingOrders_Success()
        {
            var orders = new List<Order>
            {
                new() { OrderId = 1, Status = OrderStatus.Pending, TotalAmount = 50000, OrderDate = DateTime.Today }
            };
            _mockOrderRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(orders);

            var result = await _service.ExecuteAdminOperationAsync("pending orders", "Admin");

            Assert.That(result, Does.Contain("📋 Pending Orders:"));
        }

        [Test]
        public async Task ExecuteAdminOperationAsync_UpdateCategory_Success()
        {
            var category = new Category { CategoryId = 1, CategoryName = "Electronics" };
            _mockCategoryRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(category);
            _mockCategoryRepo.Setup(x => x.UpdateAsync(It.IsAny<Category>())).Returns(Task.CompletedTask);

            var result = await _service.ExecuteAdminOperationAsync("update category 1 name Technology", "Admin");

            Assert.That(result, Does.Contain("✅ Category 'Electronics' updated to 'Technology'"));
        }

        [Test]
        public async Task ExecuteCustomerOperationAsync_UpdateExistingCartItem_Success()
        {
            var products = new List<Product>
            {
                new() { ProductId = 1, Name = "iPhone", Price = 50000, StockQuantity = 10 }
            };
            var carts = new List<Cart> { new() { CartId = 1, UserId = 1 } };
            var cartItems = new List<CartItem>
            {
                new() { CartItemId = 1, CartId = 1, ProductId = 1, Quantity = 1, TotalPrice = 50000 }
            };

            _mockProductRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(products);
            _mockCartRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(carts);
            _mockCartItemRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(cartItems);
            _mockCartItemRepo.Setup(x => x.UpdateAsync(It.IsAny<CartItem>())).Returns(Task.CompletedTask);

            var result = await _service.ExecuteCustomerOperationAsync("add 2 iPhone to cart", 1);

            Assert.That(result, Does.Contain("✅ Updated 'iPhone' in your cart! Total quantity: 3"));
        }

        [Test]
        public async Task ExecuteCustomerOperationAsync_InsufficientStock()
        {
            var products = new List<Product>
            {
                new() { ProductId = 1, Name = "iPhone", Price = 50000, StockQuantity = 1 }
            };
            _mockProductRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(products);

            var result = await _service.ExecuteCustomerOperationAsync("order 5 iPhone, address Delhi, payment mode COD", 1);

            Assert.That(result, Does.Contain("❌ Only 1 units of 'iPhone' available in stock"));
        }

        [Test]
        public async Task ExecuteCustomerOperationAsync_ProductNotFound()
        {
            _mockProductRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<Product>());

            var result = await _service.ExecuteCustomerOperationAsync("order 1 NonExistentProduct, address Delhi, payment mode COD", 1);

            Assert.That(result, Does.Contain("❌ No products found in your order request"));
        }

        [Test]
        public async Task ExecuteAdminOperationAsync_SalesReportWithNoData_ReturnsZeroStats()
        {
            _mockOrderRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<Order>());
            _mockPaymentRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<Payment>());

            var result = await _service.ExecuteAdminOperationAsync("sales report", "Admin");

            Assert.That(result, Does.Contain("💰 Sales Report:"));
            Assert.That(result, Does.Contain("Total Revenue: ₹0.00"));
        }

        [Test]
        public async Task ExecuteAdminOperationAsync_DeleteCategoryByName_Success()
        {
            var categories = new List<Category>
            {
                new() { CategoryId = 1, CategoryName = "Electronics" }
            };
            _mockCategoryRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(categories);
            _mockProductRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<Product>());
            _mockCategoryRepo.Setup(x => x.DeleteAsync(1)).Returns(Task.CompletedTask);

            var result = await _service.ExecuteAdminOperationAsync("delete category Electronics", "Admin");

            Assert.That(result, Does.Contain("✅ Category 'Electronics' deleted successfully!"));
        }

        [Test]
        public async Task ExecuteCustomerOperationAsync_ClearCartEmpty_ReturnsMessage()
        {
            _mockCartRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<Cart>());

            var result = await _service.ExecuteCustomerOperationAsync("clear cart", 1);

            Assert.That(result, Is.EqualTo("🛒 Your cart is already empty."));
        }

        [Test]
        public async Task ExecuteAdminOperationAsync_PendingOrdersEmpty_ReturnsMessage()
        {
            _mockOrderRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<Order>());

            var result = await _service.ExecuteAdminOperationAsync("pending orders", "Admin");

            Assert.That(result, Is.EqualTo("✅ No pending orders!"));
        }

        [Test]
        public async Task ExecuteAdminOperationAsync_ListCategoriesEmpty_ReturnsMessage()
        {
            _mockCategoryRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<Category>());

            var result = await _service.ExecuteAdminOperationAsync("list categories", "Admin");

            Assert.That(result, Is.EqualTo("📂 No categories found."));
        }
    }
}