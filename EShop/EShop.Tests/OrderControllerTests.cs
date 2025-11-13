using EShop.Controllers;
using EShop.Models;
using EShop.Repositories;
using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EShop.Tests
{
    public class OrderControllerTests
    {
        private Mock<IGenericRepository<Order>> _orderRepoMock;
        private Mock<IGenericRepository<OrderItem>> _orderItemRepoMock;
        private Mock<IGenericRepository<Cart>> _cartRepoMock;
        private Mock<IGenericRepository<CartItem>> _cartItemRepoMock;
        private Mock<IGenericRepository<Product>> _productRepoMock;
        private Mock<IGenericRepository<Payment>> _paymentRepoMock;
        private Mock<EShop.Services.IEmailService> _emailServiceMock;
        private OrderController _orderController;

        [SetUp]
        public void Setup()
        {
            _orderRepoMock = new Mock<IGenericRepository<Order>>();
            _orderItemRepoMock = new Mock<IGenericRepository<OrderItem>>();
            _cartRepoMock = new Mock<IGenericRepository<Cart>>();
            _cartItemRepoMock = new Mock<IGenericRepository<CartItem>>();
            _productRepoMock = new Mock<IGenericRepository<Product>>();
            _paymentRepoMock = new Mock<IGenericRepository<Payment>>();
            _emailServiceMock = new Mock<EShop.Services.IEmailService>();
            _orderController = new OrderController(_orderRepoMock.Object, _orderItemRepoMock.Object, _cartRepoMock.Object, _cartItemRepoMock.Object, _productRepoMock.Object, _paymentRepoMock.Object, _emailServiceMock.Object);
        }

        [Test]
        public async Task GetMyOrders_ReturnsOk_WithOrders()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.User = new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity(new[] {
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "123"),
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, "test@example.com"),
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "Customer")
                })
            );
            _orderController.ControllerContext = new ControllerContext { HttpContext = httpContext };
            _orderRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Order> { new Order { OrderId = 1, UserId = 123 } });
            _orderItemRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<OrderItem>());
            _productRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Product>());
            _paymentRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Payment>());
            var result = await _orderController.GetMyOrders();
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetMyOrders_ReturnsNotFound_WhenNoOrders()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.User = new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity(new[] {
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "123"),
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, "test@example.com"),
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "Customer")
                })
            );
            _orderController.ControllerContext = new ControllerContext { HttpContext = httpContext };
            _orderRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Order>());
            var result = await _orderController.GetMyOrders();
            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task GetMyOrders_ReturnsUnauthorized_WhenUserNotLoggedIn()
        {
            var httpContext = new DefaultHttpContext();
            _orderController.ControllerContext = new ControllerContext { HttpContext = httpContext };
            var result = await _orderController.GetMyOrders();
            Assert.That(result, Is.InstanceOf<UnauthorizedObjectResult>());
        }

        [Test]
        public void GetMyOrders_HandlesRepositoryException()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.User = new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity(new[] {
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "123"),
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, "test@example.com"),
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "Customer")
                })
            );
            _orderController.ControllerContext = new ControllerContext { HttpContext = httpContext };
            _orderRepoMock.Setup(r => r.GetAllAsync()).ThrowsAsync(new System.Exception("DB error"));
            Assert.ThrowsAsync<System.Exception>(async () => await _orderController.GetMyOrders());
        }

        [Test]
        public async Task PlaceOrder_ReturnsUnauthorized_WhenUserNotLoggedIn()
        {
            var httpContext = new DefaultHttpContext();
            _orderController.ControllerContext = new ControllerContext { HttpContext = httpContext };
            var orderRequest = new EShop.Dtos.OrderRequestDto { ShippingAddress = "Test", PaymentMethod = "COD" };
            var result = await _orderController.PlaceOrder(orderRequest);
            Assert.That(result, Is.InstanceOf<UnauthorizedObjectResult>());
        }

        [Test]
        public async Task PlaceOrder_ReturnsBadRequest_WhenCartNotFound()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.User = new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity(new[] {
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "123"),
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, "test@example.com")
                })
            );
            _orderController.ControllerContext = new ControllerContext { HttpContext = httpContext };
            _cartRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Cart>());
            var orderRequest = new EShop.Dtos.OrderRequestDto { ShippingAddress = "Test", PaymentMethod = "COD" };
            var result = await _orderController.PlaceOrder(orderRequest);
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task PlaceOrder_ReturnsBadRequest_WhenCartIsEmpty()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.User = new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity(new[] {
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "123"),
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, "test@example.com")
                })
            );
            _orderController.ControllerContext = new ControllerContext { HttpContext = httpContext };
            var cart = new Cart { CartId = 1, UserId = 123 };
            _cartRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([cart]);
            _cartItemRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<CartItem>());
            var orderRequest = new EShop.Dtos.OrderRequestDto { ShippingAddress = "Test", PaymentMethod = "COD" };
            var result = await _orderController.PlaceOrder(orderRequest);
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task PlaceOrder_ReturnsBadRequest_WhenInvalidPaymentMethod()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.User = new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity(new[] {
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "123"),
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, "test@example.com")
                })
            );
            _orderController.ControllerContext = new ControllerContext { HttpContext = httpContext };
            var cart = new Cart { CartId = 1, UserId = 123 };
            var cartItem = new CartItem { CartItemId = 1, CartId = 1, ProductId = 1, Quantity = 1, TotalPrice = 100 };
            _cartRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([cart]);
            _cartItemRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([cartItem]);
            var orderRequest = new EShop.Dtos.OrderRequestDto { ShippingAddress = "Test", PaymentMethod = "INVALID" };
            var result = await _orderController.PlaceOrder(orderRequest);
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task CancelOrder_ReturnsNotFound_WhenOrderNotFound()
        {
            _orderRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Order?)null);
            var result = await _orderController.CancelOrder(1);
            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task CancelOrder_ReturnsForbid_WhenUserNotOwner()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.User = new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity(new[] {
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "123")
                })
            );
            _orderController.ControllerContext = new ControllerContext { HttpContext = httpContext };
            var order = new Order { OrderId = 1, UserId = 456, Status = OrderStatus.Pending };
            _orderRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(order);
            var result = await _orderController.CancelOrder(1);
            Assert.That(result, Is.InstanceOf<ForbidResult>());
        }

        [Test]
        public async Task CancelOrder_ReturnsBadRequest_WhenOrderAlreadyShipped()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.User = new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity(new[] {
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "123")
                })
            );
            _orderController.ControllerContext = new ControllerContext { HttpContext = httpContext };
            var order = new Order { OrderId = 1, UserId = 123, Status = OrderStatus.Shipped };
            _orderRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(order);
            var result = await _orderController.CancelOrder(1);
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task CancelOrder_ReturnsOk_WhenOrderCancelledSuccessfully()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.User = new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity(new[] {
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "123"),
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, "test@test.com")
                })
            );
            _orderController.ControllerContext = new ControllerContext { HttpContext = httpContext };
            var order = new Order { OrderId = 1, UserId = 123, Status = OrderStatus.Pending };
            var orderItem = new OrderItem { OrderItemId = 1, OrderId = 1, ProductId = 1, Quantity = 2 };
            var product = new Product { ProductId = 1, StockQuantity = 5 };
            
            _orderRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(order);
            _orderItemRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([orderItem]);
            _productRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([product]);
            _paymentRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([]);
            _emailServiceMock.Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);
            
            var result = await _orderController.CancelOrder(1);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task PlaceOrder_ReturnsOk_WhenOrderPlacedSuccessfully()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.User = new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity(new[] {
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "123"),
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, "test@test.com")
                })
            );
            _orderController.ControllerContext = new ControllerContext { HttpContext = httpContext };
            var cart = new Cart { CartId = 1, UserId = 123 };
            var cartItem = new CartItem { CartItemId = 1, CartId = 1, ProductId = 1, Quantity = 2, TotalPrice = 200 };
            var product = new Product { ProductId = 1, StockQuantity = 10 };
            var orderRequest = new EShop.Dtos.OrderRequestDto { ShippingAddress = "Test Address", PaymentMethod = "COD" };
            
            _cartRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([cart]);
            _cartItemRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([cartItem]);
            _productRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([product]);
            _emailServiceMock.Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);
            
            var result = await _orderController.PlaceOrder(orderRequest);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetMyOrders_ReturnsCompleteOrderData_WithAllRelatedEntities()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.User = new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity(new[] {
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "123"),
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, "test@example.com"),
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "Customer")
                })
            );
            _orderController.ControllerContext = new ControllerContext { HttpContext = httpContext };
            
            var order = new Order { OrderId = 1, UserId = 123, OrderDate = System.DateTime.Now, TotalAmount = 200, Status = OrderStatus.Pending, ShippingAddress = "Test Address", PaymentMethod = PaymentMethod.COD };
            var orderItem = new OrderItem { OrderItemId = 1, OrderId = 1, ProductId = 1, Quantity = 2, Price = 100 };
            var product = new Product { ProductId = 1, Name = "Test Product" };
            var payment = new Payment { PaymentId = 1, OrderId = 1, Mode = PaymentMode.COD, Amount = 200, PaymentDate = System.DateTime.Now, Status = PaymentStatus.Completed };
            
            _orderRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([order]);
            _orderItemRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([orderItem]);
            _productRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([product]);
            _paymentRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([payment]);
            
            var result = await _orderController.GetMyOrders();
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
        }

        [Test]
        public async Task GetMyOrders_HandlesOrdersWithoutPayments()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.User = new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity(new[] {
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "123"),
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, "test@example.com")
                })
            );
            _orderController.ControllerContext = new ControllerContext { HttpContext = httpContext };
            
            var order = new Order { OrderId = 1, UserId = 123, OrderDate = System.DateTime.Now, TotalAmount = 100, Status = OrderStatus.Pending, ShippingAddress = "Test", PaymentMethod = PaymentMethod.COD };
            var orderItem = new OrderItem { OrderItemId = 1, OrderId = 1, ProductId = 1, Quantity = 1, Price = 100 };
            var product = new Product { ProductId = 1, Name = "Test Product" };
            
            _orderRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([order]);
            _orderItemRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([orderItem]);
            _productRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([product]);
            _paymentRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([]);
            
            var result = await _orderController.GetMyOrders();
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetMyOrders_HandlesOrdersWithMissingProducts()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.User = new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity(new[] {
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "123"),
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, "test@example.com")
                })
            );
            _orderController.ControllerContext = new ControllerContext { HttpContext = httpContext };
            
            var order = new Order { OrderId = 1, UserId = 123, OrderDate = System.DateTime.Now, TotalAmount = 100, Status = OrderStatus.Pending, ShippingAddress = "Test", PaymentMethod = PaymentMethod.COD };
            var orderItem = new OrderItem { OrderItemId = 1, OrderId = 1, ProductId = 999, Quantity = 1, Price = 100 };
            
            _orderRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([order]);
            _orderItemRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([orderItem]);
            _productRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([]);
            _paymentRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([]);
            
            var result = await _orderController.GetMyOrders();
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetMyOrders_FiltersOrdersByUserId()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.User = new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity(new[] {
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "123"),
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, "test@example.com")
                })
            );
            _orderController.ControllerContext = new ControllerContext { HttpContext = httpContext };
            
            var userOrder = new Order { OrderId = 1, UserId = 123, OrderDate = System.DateTime.Now, TotalAmount = 100, Status = OrderStatus.Pending, ShippingAddress = "Test", PaymentMethod = PaymentMethod.COD };
            var otherUserOrder = new Order { OrderId = 2, UserId = 456, OrderDate = System.DateTime.Now, TotalAmount = 200, Status = OrderStatus.Pending, ShippingAddress = "Test2", PaymentMethod = PaymentMethod.COD };
            
            _orderRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([userOrder, otherUserOrder]);
            _orderItemRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([]);
            _productRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([]);
            _paymentRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([]);
            
            var result = await _orderController.GetMyOrders();
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }
    }
}
