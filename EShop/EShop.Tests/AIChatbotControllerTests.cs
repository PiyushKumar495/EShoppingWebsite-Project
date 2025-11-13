using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using EShop.Controllers;
using EShop.Services;
using System.Security.Claims;
using System.Net;

namespace EShop.Tests
{
    public class AIChatbotControllerTests
    {
        private Mock<IAzureOpenAIService> _mockOpenAIService;
        private Mock<IChatbotOperationsService> _mockOperationsService;
        private AIChatbotController _controller;

        [SetUp]
        public void Setup()
        {
            _mockOpenAIService = new Mock<IAzureOpenAIService>();
            _mockOperationsService = new Mock<IChatbotOperationsService>();
            _controller = new AIChatbotController(_mockOpenAIService.Object, _mockOperationsService.Object);
        }

        [Test]
        public async Task Chat_WithOrderStatusQuery_UnauthenticatedUser_ReturnsLoginMessage()
        {
            var request = new ChatRequest { Message = "check order status 123" };
            SetupUnauthenticatedUser();

            var result = await _controller.Chat(request);

            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            var response = okResult.Value;
            var replyProperty = response?.GetType().GetProperty("reply");
            var reply = replyProperty?.GetValue(response)?.ToString();
            Assert.That(reply, Is.EqualTo("❌ Please login to check your order status."));
        }

        [Test]
        public async Task Chat_WithOrderStatusQuery_AuthenticatedUser_CallsOperationsService()
        {
            var request = new ChatRequest { Message = "order status 456" };
            var expectedResponse = "Order 456 is shipped";
            SetupAuthenticatedUser("1", "Customer");
            _mockOperationsService.Setup(x => x.ExecuteCustomerOperationAsync(request.Message, 1))
                .ReturnsAsync(expectedResponse);

            var result = await _controller.Chat(request);

            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            var response = okResult.Value;
            var replyProperty = response?.GetType().GetProperty("reply");
            var reply = replyProperty?.GetValue(response)?.ToString();
            Assert.That(reply, Is.EqualTo(expectedResponse));
        }

        [Test]
        public async Task Chat_WithAdminOperation_ValidAdmin_CallsAdminOperations()
        {
            var request = new ChatRequest { Message = "add product iPhone price 75000 stock 50 category Electronics" };
            var expectedResponse = "Product added successfully";
            SetupAuthenticatedUser("1", "Admin");
            _mockOperationsService.Setup(x => x.ExecuteAdminOperationAsync(request.Message, "Admin"))
                .ReturnsAsync(expectedResponse);

            var result = await _controller.Chat(request);

            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            var response = okResult.Value;
            var replyProperty = response?.GetType().GetProperty("reply");
            var reply = replyProperty?.GetValue(response)?.ToString();
            Assert.That(reply, Is.EqualTo(expectedResponse));
        }

        [Test]
        public async Task Chat_WithCustomerOperation_CallsCustomerOperations()
        {
            var request = new ChatRequest { Message = "show products" };
            var expectedResponse = "Here are the products...";
            SetupAuthenticatedUser("1", "Customer");
            _mockOperationsService.Setup(x => x.ExecuteCustomerOperationAsync(request.Message, 1))
                .ReturnsAsync(expectedResponse);

            var result = await _controller.Chat(request);

            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            var response = okResult.Value;
            var replyProperty = response?.GetType().GetProperty("reply");
            var reply = replyProperty?.GetValue(response)?.ToString();
            Assert.That(reply, Is.EqualTo(expectedResponse));
        }

        [Test]
        public async Task Chat_WithGeneralQuery_CallsOpenAIService()
        {
            var request = new ChatRequest { Message = "What is your return policy?" };
            var expectedResponse = "Our return policy is...";
            SetupUnauthenticatedUser();
            _mockOpenAIService.Setup(x => x.GetChatResponseAsync(request.Message, It.IsAny<List<string>>()))
                .ReturnsAsync(expectedResponse);

            var result = await _controller.Chat(request);

            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            var response = okResult.Value;
            var replyProperty = response?.GetType().GetProperty("reply");
            var reply = replyProperty?.GetValue(response)?.ToString();
            Assert.That(reply, Is.EqualTo(expectedResponse));
        }

        [TestCase("add product test")]
        [TestCase("update product 1")]
        [TestCase("delete product iPhone")]
        [TestCase("add category Electronics")]
        [TestCase("update category 1")]
        [TestCase("delete category Electronics")]
        [TestCase("low stock")]
        [TestCase("pending orders")]
        [TestCase("order statistics")]
        [TestCase("sales report")]
        [TestCase("user statistics")]
        [TestCase("inventory report")]
        [TestCase("update order 123")]
        [TestCase("list categories")]
        [TestCase("delete iPhone")]
        [TestCase("iPhone,75000,50,Electronics")]
        [TestCase("1,80000")]
        [TestCase("Electronics,NewName")]
        public async Task Chat_WithAdminOperations_IdentifiesAsAdminOperation(string message)
        {
            var request = new ChatRequest { Message = message };
            SetupAuthenticatedUser("1", "Admin");
            _mockOperationsService.Setup(x => x.ExecuteAdminOperationAsync(message, "Admin"))
                .ReturnsAsync("Admin operation executed");

            var result = await _controller.Chat(request);

            _mockOperationsService.Verify(x => x.ExecuteAdminOperationAsync(message, "Admin"), Times.Once);
        }

        [TestCase("show products")]
        [TestCase("find iPhone")]
        [TestCase("search electronics")]
        [TestCase("order check 123")]
        [TestCase("cancel order 12")]
        [TestCase("add iPhone to cart")]
        [TestCase("add 2 iPhone cart")]
        [TestCase("view cart")]
        [TestCase("show cart")]
        [TestCase("my cart")]
        [TestCase("order 5 jeans, address lucknow, payment mode cod")]
        [TestCase("place order 2 iPhone, address delhi, payment mode online")]
        [TestCase("order cart, address mumbai, payment mode cod")]
        [TestCase("checkout, address pune, payment mode online")]
        public async Task Chat_WithCustomerOperations_IdentifiesAsCustomerOperation(string message)
        {
            var request = new ChatRequest { Message = message };
            SetupAuthenticatedUser("1", "Customer");
            _mockOperationsService.Setup(x => x.ExecuteCustomerOperationAsync(message, 1))
                .ReturnsAsync("Customer operation executed");

            var result = await _controller.Chat(request);

            _mockOperationsService.Verify(x => x.ExecuteCustomerOperationAsync(message, 1), Times.Once);
        }

        [Test]
        public async Task Chat_WithInvalidUserId_PassesZeroToCustomerOperation()
        {
            var request = new ChatRequest { Message = "show products" };
            SetupAuthenticatedUserWithInvalidId("invalid", "Customer");
            _mockOperationsService.Setup(x => x.ExecuteCustomerOperationAsync(request.Message, 0))
                .ReturnsAsync("Operation executed");

            var result = await _controller.Chat(request);

            _mockOperationsService.Verify(x => x.ExecuteCustomerOperationAsync(request.Message, 0), Times.Once);
        }

        [Test]
        public async Task Chat_ConversationHistoryLimit_TrimsToTenMessages()
        {
            SetupUnauthenticatedUser();
            _mockOpenAIService.Setup(x => x.GetChatResponseAsync(It.IsAny<string>(), It.IsAny<List<string>>()))
                .ReturnsAsync("Response");

            for (int i = 0; i < 12; i++)
            {
                await _controller.Chat(new ChatRequest { Message = $"Message {i}" });
            }

            _mockOpenAIService.Verify(x => x.GetChatResponseAsync(It.IsAny<string>(), 
                It.Is<List<string>>(h => h.Count <= 10)), Times.AtLeast(1));
        }

        [Test]
        public async Task StreamChat_WithMessage_CallsStreamingService()
        {
            var request = new ChatRequest { Message = "Hello" };
            var expectedResponse = "Streaming response";
            SetupUnauthenticatedUser();
            _mockOpenAIService.Setup(x => x.GetStreamingChatResponseAsync(request.Message, It.IsAny<List<string>>()))
                .ReturnsAsync(expectedResponse);

            var result = await _controller.StreamChat(request);

            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            var response = okResult.Value;
            var replyProperty = response?.GetType().GetProperty("reply");
            var reply = replyProperty?.GetValue(response)?.ToString();
            Assert.That(reply, Is.EqualTo(expectedResponse));
        }

        #region Edge Cases and Error Handling

        [Test]
        public void Chat_WithNullRequest_ShouldHandleGracefully()
        {
            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(async () => await _controller.Chat(null!));
        }

        [Test]
        public void Chat_WithNullMessage_ShouldHandleGracefully()
        {
            // Arrange
            var request = new ChatRequest { Message = null! };

            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(async () => await _controller.Chat(request));
        }

        [Test]
        public void Chat_WithEmptyMessage_ShouldHandleGracefully()
        {
            // Arrange
            var request = new ChatRequest { Message = "" };

            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(async () => await _controller.Chat(request));
        }

        [Test]
        public async Task Chat_WithWhitespaceMessage_ShouldCallOpenAI()
        {
            // Arrange
            var request = new ChatRequest { Message = "   " };
            SetupUnauthenticatedUser();
            _mockOpenAIService.Setup(x => x.GetChatResponseAsync(request.Message, It.IsAny<List<string>>()))
                .ReturnsAsync("Response to whitespace");

            // Act
            var result = await _controller.Chat(request);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
        }

        [Test]
        public async Task Chat_WithVeryLongMessage_ShouldHandleGracefully()
        {
            // Arrange
            var longMessage = new string('a', 10000);
            var request = new ChatRequest { Message = longMessage };
            SetupUnauthenticatedUser();
            _mockOpenAIService.Setup(x => x.GetChatResponseAsync(longMessage, It.IsAny<List<string>>()))
                .ReturnsAsync("Response to long message");

            // Act
            var result = await _controller.Chat(request);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
        }

        [Test]
        public async Task Chat_WithSpecialCharacters_ShouldHandleGracefully()
        {
            // Arrange
            var specialMessage = "!@#$%^&*()_+{}|:<>?[]\\;'\",./<>?";
            var request = new ChatRequest { Message = specialMessage };
            SetupUnauthenticatedUser();
            _mockOpenAIService.Setup(x => x.GetChatResponseAsync(specialMessage, It.IsAny<List<string>>()))
                .ReturnsAsync("Response to special characters");

            // Act
            var result = await _controller.Chat(request);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
        }

        [Test]
        public async Task Chat_WithUnicodeCharacters_ShouldHandleGracefully()
        {
            // Arrange
            var unicodeMessage = "测试消息 🛒🛍️💰 العربية русский";
            var request = new ChatRequest { Message = unicodeMessage };
            SetupUnauthenticatedUser();
            _mockOpenAIService.Setup(x => x.GetChatResponseAsync(unicodeMessage, It.IsAny<List<string>>()))
                .ReturnsAsync("Response to unicode");

            // Act
            var result = await _controller.Chat(request);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
        }

        [Test]
        public void Chat_WithOpenAIServiceException_ShouldThrowException()
        {
            // Arrange
            var request = new ChatRequest { Message = "test message" };
            SetupUnauthenticatedUser();
            _mockOpenAIService.Setup(x => x.GetChatResponseAsync(It.IsAny<string>(), It.IsAny<List<string>>()))
                .ThrowsAsync(new InvalidOperationException("OpenAI service error"));

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () => await _controller.Chat(request));
        }

        [Test]
        public void Chat_WithOperationsServiceException_ShouldThrowException()
        {
            // Arrange
            var request = new ChatRequest { Message = "add product test" };
            SetupAuthenticatedUser("1", "Admin");
            _mockOperationsService.Setup(x => x.ExecuteAdminOperationAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException("Operations service error"));

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () => await _controller.Chat(request));
        }

        [Test]
        public async Task Chat_WithNullUserIdClaim_ShouldPassZeroToOperations()
        {
            // Arrange
            var request = new ChatRequest { Message = "show products" };
            SetupAuthenticatedUserWithNullId("Customer");
            _mockOperationsService.Setup(x => x.ExecuteCustomerOperationAsync(request.Message, 0))
                .ReturnsAsync("Operation executed");

            // Act
            var result = await _controller.Chat(request);

            // Assert
            _mockOperationsService.Verify(x => x.ExecuteCustomerOperationAsync(request.Message, 0), Times.Once);
        }

        [Test]
        public async Task Chat_WithEmptyUserIdClaim_ShouldPassZeroToOperations()
        {
            // Arrange
            var request = new ChatRequest { Message = "show products" };
            SetupAuthenticatedUserWithEmptyId("Customer");
            _mockOperationsService.Setup(x => x.ExecuteCustomerOperationAsync(request.Message, 0))
                .ReturnsAsync("Operation executed");

            // Act
            var result = await _controller.Chat(request);

            // Assert
            _mockOperationsService.Verify(x => x.ExecuteCustomerOperationAsync(request.Message, 0), Times.Once);
        }

        [Test]
        public async Task Chat_WithNullRoleClaim_ShouldUseOpenAIService()
        {
            // Arrange
            var request = new ChatRequest { Message = "test message" };
            SetupAuthenticatedUserWithNullRole("1");
            _mockOpenAIService.Setup(x => x.GetChatResponseAsync(request.Message, It.IsAny<List<string>>()))
                .ReturnsAsync("OpenAI response");

            // Act
            var result = await _controller.Chat(request);

            // Assert
            _mockOpenAIService.Verify(x => x.GetChatResponseAsync(request.Message, It.IsAny<List<string>>()), Times.Once);
        }

        [Test]
        public async Task Chat_WithEmptyRoleClaim_ShouldUseOpenAIService()
        {
            // Arrange
            var request = new ChatRequest { Message = "test message" };
            SetupAuthenticatedUserWithEmptyRole("1");
            _mockOpenAIService.Setup(x => x.GetChatResponseAsync(request.Message, It.IsAny<List<string>>()))
                .ReturnsAsync("OpenAI response");

            // Act
            var result = await _controller.Chat(request);

            // Assert
            _mockOpenAIService.Verify(x => x.GetChatResponseAsync(request.Message, It.IsAny<List<string>>()), Times.Once);
        }

        [Test]
        public async Task Chat_WithUnknownRole_ShouldUseOpenAIService()
        {
            // Arrange
            var request = new ChatRequest { Message = "test message" };
            SetupAuthenticatedUser("1", "UnknownRole");
            _mockOpenAIService.Setup(x => x.GetChatResponseAsync(request.Message, It.IsAny<List<string>>()))
                .ReturnsAsync("OpenAI response");

            // Act
            var result = await _controller.Chat(request);

            // Assert
            _mockOpenAIService.Verify(x => x.GetChatResponseAsync(request.Message, It.IsAny<List<string>>()), Times.Once);
        }

        [Test]
        public void StreamChat_WithNullRequest_ShouldThrowException()
        {
            // Act & Assert
            Assert.ThrowsAsync<NullReferenceException>(async () => await _controller.StreamChat(null!));
        }

        [Test]
        public async Task StreamChat_WithNullMessage_ShouldHandleGracefully()
        {
            // Arrange
            var request = new ChatRequest { Message = null! };
            SetupUnauthenticatedUser();
            _mockOpenAIService.Setup(x => x.GetStreamingChatResponseAsync(null!, It.IsAny<List<string>>()))
                .ReturnsAsync("Please provide a message");

            // Act
            var result = await _controller.StreamChat(request);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
        }

        [Test]
        public async Task StreamChat_WithEmptyMessage_ShouldHandleGracefully()
        {
            // Arrange
            var request = new ChatRequest { Message = "" };
            SetupUnauthenticatedUser();
            _mockOpenAIService.Setup(x => x.GetStreamingChatResponseAsync("", It.IsAny<List<string>>()))
                .ReturnsAsync("Please provide a message");

            // Act
            var result = await _controller.StreamChat(request);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
        }

        [Test]
        public void StreamChat_WithStreamingServiceException_ShouldThrowException()
        {
            // Arrange
            var request = new ChatRequest { Message = "test message" };
            SetupUnauthenticatedUser();
            _mockOpenAIService.Setup(x => x.GetStreamingChatResponseAsync(It.IsAny<string>(), It.IsAny<List<string>>()))
                .ThrowsAsync(new InvalidOperationException("Streaming service error"));

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () => await _controller.StreamChat(request));
        }

        [TestCase("order status")]
        [TestCase("check order")]
        [TestCase("order check")]
        [TestCase("status order")]
        public async Task Chat_WithOrderStatusVariations_UnauthenticatedUser_ReturnsLoginMessage(string message)
        {
            var request = new ChatRequest { Message = message };
            SetupUnauthenticatedUser();

            var result = await _controller.Chat(request);

            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            var response = okResult.Value;
            var replyProperty = response?.GetType().GetProperty("reply");
            var reply = replyProperty?.GetValue(response)?.ToString();
            Assert.That(reply, Is.EqualTo("❌ Please login to check your order status."));
        }

        [TestCase("ADMIN")]
        [TestCase("admin")]
        [TestCase("Admin")]
        [TestCase("aDmIn")]
        public async Task Chat_WithCaseInsensitiveAdminRole_CallsAdminOperations(string role)
        {
            var request = new ChatRequest { Message = "add product test" };
            SetupAuthenticatedUser("1", role);
            _mockOperationsService.Setup(x => x.ExecuteAdminOperationAsync(request.Message, role))
                .ReturnsAsync("Admin operation executed");

            var result = await _controller.Chat(request);

            _mockOperationsService.Verify(x => x.ExecuteAdminOperationAsync(request.Message, role), Times.Once);
        }

        [TestCase("CUSTOMER")]
        [TestCase("customer")]
        [TestCase("Customer")]
        [TestCase("cUsToMeR")]
        public async Task Chat_WithCaseInsensitiveCustomerRole_CallsCustomerOperations(string role)
        {
            var request = new ChatRequest { Message = "show products" };
            SetupAuthenticatedUser("1", role);
            _mockOperationsService.Setup(x => x.ExecuteCustomerOperationAsync(request.Message, 1))
                .ReturnsAsync("Customer operation executed");

            var result = await _controller.Chat(request);

            _mockOperationsService.Verify(x => x.ExecuteCustomerOperationAsync(request.Message, 1), Times.Once);
        }

        #endregion

        #region Helper Methods

        private void SetupAuthenticatedUser(string userId, string role)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Role, role)
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        private void SetupAuthenticatedUserWithInvalidId(string userId, string role)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Role, role)
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        private void SetupUnauthenticatedUser()
        {
            var identity = new ClaimsIdentity();
            var principal = new ClaimsPrincipal(identity);
            
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        private void SetupAuthenticatedUserWithNullId(string role)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Role, role)
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        private void SetupAuthenticatedUserWithEmptyId(string role)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, ""),
                new Claim(ClaimTypes.Role, role)
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        private void SetupAuthenticatedUserWithNullRole(string userId)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        private void SetupAuthenticatedUserWithEmptyRole(string userId)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Role, "")
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        #endregion
    }
}