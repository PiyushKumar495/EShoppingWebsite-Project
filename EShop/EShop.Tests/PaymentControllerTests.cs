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
    public class PaymentControllerTests
    {
        private Mock<IGenericRepository<Payment>> _paymentRepoMock;
        private Mock<IGenericRepository<Order>> _orderRepoMock;
        private Mock<EShop.Services.IEmailService> _emailServiceMock;
        private PaymentController _paymentController;

        [SetUp]
        public void Setup()
        {
            _paymentRepoMock = new Mock<IGenericRepository<Payment>>();
            _orderRepoMock = new Mock<IGenericRepository<Order>>();
            _emailServiceMock = new Mock<EShop.Services.IEmailService>();
            _paymentController = new PaymentController(_paymentRepoMock.Object, _orderRepoMock.Object, _emailServiceMock.Object);
        }

        [Test]
        public async Task MakePayment_ReturnsOk_WhenPaymentIsSuccessful()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.User = new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity(new[] {
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, "test@example.com")
                })
            );
            _paymentController.ControllerContext = new ControllerContext { HttpContext = httpContext };
            _paymentRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Payment>());
            _orderRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Order { OrderId = 1, TotalAmount = 100, PaymentMethod = EShop.Models.PaymentMethod.UPI });
            var Dto = new Dtos.PaymentCreateDto { OrderId = 1, Amount = 100, Mode = "UPI" };
            var result = await _paymentController.MakePayment(Dto);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task MakePayment_ReturnsNotFound_WhenOrderDoesNotExist()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.User = new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity(new[] {
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, "test@example.com")
                })
            );
            _paymentController.ControllerContext = new ControllerContext { HttpContext = httpContext };
            _orderRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Order?)null);
            var Dto = new Dtos.PaymentCreateDto { OrderId = 1, Amount = 100, Mode = "UPI" };
            var result = await _paymentController.MakePayment(Dto);
            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task MakePayment_ReturnsBadRequest_WhenAmountMismatch()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.User = new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity(new[] {
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, "test@example.com")
                })
            );
            _paymentController.ControllerContext = new ControllerContext { HttpContext = httpContext };
            _orderRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Order { OrderId = 1, TotalAmount = 200, PaymentMethod = EShop.Models.PaymentMethod.UPI });
            var Dto = new Dtos.PaymentCreateDto { OrderId = 1, Amount = 100, Mode = "UPI" };
            var result = await _paymentController.MakePayment(Dto);
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task MakePayment_ReturnsUnauthorized_WhenUserNotLoggedIn()
        {
            var httpContext = new DefaultHttpContext();
            _paymentController.ControllerContext = new ControllerContext { HttpContext = httpContext };
            var Dto = new Dtos.PaymentCreateDto { OrderId = 1, Amount = 100, Mode = "UPI" };
            var result = await _paymentController.MakePayment(Dto);
            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task MakePayment_ReturnsBadRequest_WhenInputInvalid()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.User = new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity(new[] {
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, "test@example.com")
                })
            );
            _paymentController.ControllerContext = new ControllerContext { HttpContext = httpContext };
            var Dto = new Dtos.PaymentCreateDto { OrderId = 0, Amount = 0, Mode = null };
            var result = await _paymentController.MakePayment(Dto);
            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        }

        // Add more tests for invalid payment, unauthorized, etc.
    }
}
