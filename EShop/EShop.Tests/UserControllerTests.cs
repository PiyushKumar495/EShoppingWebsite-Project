using EShop.Controllers;
using EShop.Models;
using EShop.Repositories;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EShop.Tests
{
    public class UserControllerTests
    {
        private Mock<IGenericRepository<Order>> _orderRepoMock;
        private Mock<IGenericRepository<OrderItem>> _orderItemRepoMock;
        private Mock<IGenericRepository<Payment>> _paymentRepoMock;
        private UserController _userController;

        [SetUp]
        public void Setup()
        {
            _orderRepoMock = new Mock<IGenericRepository<Order>>();
            _orderItemRepoMock = new Mock<IGenericRepository<OrderItem>>();
            _paymentRepoMock = new Mock<IGenericRepository<Payment>>();
            _userController = new UserController(_orderRepoMock.Object, _orderItemRepoMock.Object, _paymentRepoMock.Object);
        }

        [Test]
        public async Task GetUserOrders_ReturnsOk_WhenOrdersExist()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.User = new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity(new[] {
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "1")
                })
            );
            _userController.ControllerContext = new ControllerContext { HttpContext = httpContext };
            var orders = new List<Order> { new Order { OrderId = 1, UserId = 1 } };
            _orderRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(orders);
            _orderItemRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<OrderItem>());
            _paymentRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Payment>());
            var result = await _userController.GetUserOrders();
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetUserOrders_ReturnsNotFound_WhenNoOrders()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.User = new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity(new[] {
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "1")
                })
            );
            _userController.ControllerContext = new ControllerContext { HttpContext = httpContext };
            _orderRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Order>());
            var result = await _userController.GetUserOrders();
            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task GetUserOrders_ReturnsUnauthorized_WhenUserNotLoggedIn()
        {
            var httpContext = new DefaultHttpContext();
            _userController.ControllerContext = new ControllerContext { HttpContext = httpContext };
            var result = await _userController.GetUserOrders();
            Assert.That(result, Is.InstanceOf<UnauthorizedObjectResult>());
        }

        [Test]
        public async Task GetUserPayments_ReturnsOk_WhenPaymentsExist()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.User = new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity(new[] {
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "1")
                })
            );
            _userController.ControllerContext = new ControllerContext { HttpContext = httpContext };
            var orders = new List<Order> { new Order { OrderId = 1, UserId = 1 } };
            var payments = new List<Payment> { new Payment { PaymentId = 1, OrderId = 1, Amount = 100 } };
            _orderRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(orders);
            _paymentRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(payments);
            var result = await _userController.GetUserPayments();
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetUserPayments_ReturnsNotFound_WhenNoPayments()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.User = new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity(new[] {
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "1")
                })
            );
            _userController.ControllerContext = new ControllerContext { HttpContext = httpContext };
            var orders = new List<Order> { new Order { OrderId = 1, UserId = 1 } };
            _orderRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(orders);
            _paymentRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Payment>());
            var result = await _userController.GetUserPayments();
            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task GetUserPayments_ReturnsUnauthorized_WhenUserNotLoggedIn()
        {
            var httpContext = new DefaultHttpContext();
            _userController.ControllerContext = new ControllerContext { HttpContext = httpContext };
            var result = await _userController.GetUserPayments();
            Assert.That(result, Is.InstanceOf<UnauthorizedObjectResult>());
        }

        // Add more tests for edge cases, nulls, and error handling as needed to reach high coverage.
    }
}
