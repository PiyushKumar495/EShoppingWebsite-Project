using NUnit.Framework;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Collections.Generic;
using System.Threading.Tasks;
using EShop.Controllers;
using EShop.Models;
using EShop.Dtos;
using EShop.Repositories;
using System.Linq;

namespace EShop.Tests
{
    public class AdminControllerTests
    {
        private Mock<IGenericRepository<Order>> _orderRepoMock;
        private Mock<IGenericRepository<OrderItem>> _orderItemRepoMock;
        private Mock<IGenericRepository<Payment>> _paymentRepoMock;
        private Mock<IGenericRepository<Product>> _productRepoMock;
        private Mock<IGenericRepository<User>> _userRepoMock;
        private AdminController _controller;

        [SetUp]
        public void Setup()
        {
            _orderRepoMock = new Mock<IGenericRepository<Order>>();
            _orderItemRepoMock = new Mock<IGenericRepository<OrderItem>>();
            _paymentRepoMock = new Mock<IGenericRepository<Payment>>();
            _productRepoMock = new Mock<IGenericRepository<Product>>();
            _userRepoMock = new Mock<IGenericRepository<User>>();
            _controller = new AdminController(
                _orderRepoMock.Object,
                _orderItemRepoMock.Object,
                _paymentRepoMock.Object,
                _productRepoMock.Object,
                _userRepoMock.Object
            );
        }

        private void SetAdminUser()
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Role, "Admin"),
                new("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Admin")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var user = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new()
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Test]
        public async Task GetAllOrders_ReturnsOk_WhenOrdersExist()
        {
            SetAdminUser();
            _orderRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([
                new Order { OrderId = 1, UserId = 1, OrderDate = System.DateTime.Now, TotalAmount = 100, Status = OrderStatus.Pending, ShippingAddress = "A", PaymentMethod = PaymentMethod.COD }
            ]);
            _orderItemRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([]);
            _paymentRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([]);
            _productRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([]);
            _userRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([new User { UserId = 1, FullName = "Admin" }]);

            var result = await _controller.GetAllOrders();
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetAllOrders_ReturnsNotFound_WhenNoOrders()
        {
            SetAdminUser();
            _orderRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([]);
            var result = await _controller.GetAllOrders();
            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task GetAllOrders_ReturnsForbid_WhenNotAdmin()
        {
            // No admin claim
            _controller.ControllerContext = new()
            {
                HttpContext = new DefaultHttpContext { User = new() }
            };
            var result = await _controller.GetAllOrders();
            Assert.That(result, Is.InstanceOf<ForbidResult>());
        }

        [Test]
        public async Task UpdateOrderStatus_ReturnsNotFound_WhenOrderNotFound()
        {
            _orderRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([]);
            var result = await _controller.UpdateOrderStatus(1, "Delivered");
            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task UpdateOrderStatus_ReturnsBadRequest_WhenStatusInvalid()
        {
            _orderRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([new Order { OrderId = 1 }]);
            var result = await _controller.UpdateOrderStatus(1, "InvalidStatus");
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task GetAllPayments_ReturnsOk()
        {
            SetAdminUser();
            _paymentRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([
                new Payment { PaymentId = 1, OrderId = 1, Mode = (PaymentMode)PaymentMethod.COD, Amount = 100, PaymentDate = System.DateTime.Now, Status = PaymentStatus.Completed }
            ]);
            var result = await _controller.GetAllPayments();
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task UpdateOrderStatus_ReturnsOk_WhenStatusUpdated()
        {
            var order = new Order { OrderId = 1, Status = OrderStatus.Pending };
            _orderRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([order]);
            _orderRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Order>())).Returns(Task.CompletedTask);
            _paymentRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([]);

            var result = await _controller.UpdateOrderStatus(1, "Shipped");
            Assert.That(result, Is.InstanceOf<JsonResult>());
        }

        [Test]
        public async Task UpdateOrderStatus_ReturnsBadRequest_WhenDeliveredWithoutPayment()
        {
            var order = new Order { OrderId = 1, Status = OrderStatus.Pending };
            _orderRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([order]);
            _paymentRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([]);

            var result = await _controller.UpdateOrderStatus(1, "Delivered");
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task UpdateOrderStatus_UpdatesCODPayment_WhenDelivered()
        {
            var order = new Order { OrderId = 1, Status = OrderStatus.Pending };
            var payment = new Payment { PaymentId = 1, OrderId = 1, Mode = PaymentMode.COD, Status = PaymentStatus.Pending };
            _orderRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([order]);
            _paymentRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([payment]);
            _orderRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Order>())).Returns(Task.CompletedTask);
            _paymentRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Payment>())).Returns(Task.CompletedTask);

            var result = await _controller.UpdateOrderStatus(1, "Delivered");
            Assert.That(result, Is.InstanceOf<JsonResult>());
        }

        [Test]
        public async Task UpdateOrderStatus_RefundsPayment_WhenCancelled()
        {
            var order = new Order { OrderId = 1, Status = OrderStatus.Pending };
            var payment = new Payment { PaymentId = 1, OrderId = 1, Mode = PaymentMode.UPI, Status = PaymentStatus.Completed };
            _orderRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([order]);
            _paymentRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([payment]);
            _orderRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Order>())).Returns(Task.CompletedTask);
            _paymentRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Payment>())).Returns(Task.CompletedTask);

            var result = await _controller.UpdateOrderStatus(1, "Cancelled");
            Assert.That(result, Is.InstanceOf<JsonResult>());
        }

        [Test]
        public async Task GetAllOrders_HandlesEmptyUserList()
        {
            SetAdminUser();
            _orderRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([new Order { OrderId = 1, UserId = 1 }]);
            _orderItemRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([]);
            _paymentRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([]);
            _productRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([]);
            _userRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([]);

            var result = await _controller.GetAllOrders();
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task UpdateOrderStatus_DoesNotRefund_WhenPaymentAlreadyRefunded()
        {
            var order = new Order { OrderId = 1, Status = OrderStatus.Pending };
            var payment = new Payment { PaymentId = 1, OrderId = 1, Mode = PaymentMode.UPI, Status = PaymentStatus.Refund };
            _orderRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([order]);
            _paymentRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([payment]);
            _orderRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Order>())).Returns(Task.CompletedTask);

            var result = await _controller.UpdateOrderStatus(1, "Cancelled");
            Assert.That(result, Is.InstanceOf<JsonResult>());
        }

        [Test]
        public async Task UpdateOrderStatus_ReturnsBadRequest_WhenUPIPaymentNotCompleted()
        {
            var order = new Order { OrderId = 1, Status = OrderStatus.Pending };
            var payment = new Payment { PaymentId = 1, OrderId = 1, Mode = PaymentMode.UPI, Status = PaymentStatus.Pending };
            _orderRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([order]);
            _paymentRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([payment]);

            var result = await _controller.UpdateOrderStatus(1, "Delivered");
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task GetAllOrders_ReturnsCompleteOrderData_WithAllRelatedEntities()
        {
            SetAdminUser();
            var order = new Order { OrderId = 1, UserId = 1, OrderDate = System.DateTime.Now, TotalAmount = 100, Status = OrderStatus.Pending, ShippingAddress = "Test Address", PaymentMethod = PaymentMethod.COD };
            var orderItem = new OrderItem { OrderItemId = 1, OrderId = 1, ProductId = 1, Quantity = 2, Price = 50 };
            var payment = new Payment { PaymentId = 1, OrderId = 1, Mode = PaymentMode.COD, Amount = 100, PaymentDate = System.DateTime.Now, Status = PaymentStatus.Completed };
            var product = new Product { ProductId = 1, Name = "Test Product" };
            var user = new User { UserId = 1, FullName = "Test User" };

            _orderRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([order]);
            _orderItemRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([orderItem]);
            _paymentRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([payment]);
            _productRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([product]);
            _userRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([user]);

            var result = await _controller.GetAllOrders();
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
        }

        [Test]
        public async Task GetAllOrders_HandlesOrdersWithoutPayments()
        {
            SetAdminUser();
            var order = new Order { OrderId = 1, UserId = 1, OrderDate = System.DateTime.Now, TotalAmount = 100, Status = OrderStatus.Pending, ShippingAddress = "Test", PaymentMethod = PaymentMethod.COD };
            
            _orderRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([order]);
            _orderItemRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([]);
            _paymentRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([]);
            _productRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([]);
            _userRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([]);

            var result = await _controller.GetAllOrders();
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetAllOrders_HandlesOrdersWithoutOrderItems()
        {
            SetAdminUser();
            var order = new Order { OrderId = 1, UserId = 1, OrderDate = System.DateTime.Now, TotalAmount = 100, Status = OrderStatus.Pending, ShippingAddress = "Test", PaymentMethod = PaymentMethod.COD };
            var payment = new Payment { PaymentId = 1, OrderId = 1, Mode = PaymentMode.COD, Amount = 100, PaymentDate = System.DateTime.Now, Status = PaymentStatus.Completed };
            
            _orderRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([order]);
            _orderItemRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([]);
            _paymentRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([payment]);
            _productRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([]);
            _userRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([]);

            var result = await _controller.GetAllOrders();
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetAllOrders_ReturnsForbid_WhenUserNotAdmin()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.User = new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity(new[] {
                    new System.Security.Claims.Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Customer")
                })
            );
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            var result = await _controller.GetAllOrders();
            Assert.That(result, Is.InstanceOf<ForbidResult>());
        }
    }
}