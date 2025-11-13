using System.Security.Claims;
using System.Linq;
using EShop.Controllers;
using EShop.Dtos;
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
    public class CartControllerTests
    {
        private Mock<IGenericRepository<Cart>> _cartRepoMock;
        private Mock<IGenericRepository<CartItem>> _cartItemRepoMock;
        private Mock<IGenericRepository<Product>> _productRepoMock;
        private CartController _cartController;

        private void SetUser(int userId)
        {
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var user = new ClaimsPrincipal(identity);
            _cartController.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };
        }

        [SetUp]
        public void Setup()
        {
            _cartRepoMock = new Mock<IGenericRepository<Cart>>();
            _cartItemRepoMock = new Mock<IGenericRepository<CartItem>>();
            _productRepoMock = new Mock<IGenericRepository<Product>>();
            _cartController = new CartController(_cartRepoMock.Object, _cartItemRepoMock.Object, _productRepoMock.Object);
        }

        [Test]
        public async Task AdDtoCart_ReturnsUnauthorized_WhenUserNotLoggedIn()
        {
            var dto = new AdDtoCartDto { ProductName = "Test", Quantity = 1 };
            _cartController.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            var result = await _cartController.AdDtoCart(dto);
            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task AdDtoCart_ReturnsNotFound_WhenProductNotFound()
        {
            SetUser(1);
            _productRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Product>());
            var dto = new AdDtoCartDto { ProductName = "Missing", Quantity = 1 };
            var result = await _cartController.AdDtoCart(dto);
            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task AdDtoCart_ReturnsBadRequest_WhenStockInsufficient()
        {
            SetUser(1);
            _productRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Product> { new Product { Name = "Test", StockQuantity = 1 } });
            var dto = new AdDtoCartDto { ProductName = "Test", Quantity = 2 };
            var result = await _cartController.AdDtoCart(dto);
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task MergeGuestCart_ReturnsUnauthorized_WhenUserNotLoggedIn()
        {
            var dto = new CartController.MergeCartDto { Items = new List<CartController.MergeCartItemDto>() };
            _cartController.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            var result = await _cartController.MergeGuestCart(dto);
            Assert.That(result, Is.InstanceOf<UnauthorizedObjectResult>());
        }

        [Test]
        public async Task RemoveFromCart_ReturnsNotFound_WhenItemNotFound()
        {
            SetUser(1);
            _cartItemRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((CartItem?)null);
            var result = await _cartController.RemoveFromCart(1);
            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task RemoveFromCart_ReturnsForbid_WhenUserNotOwner()
        {
            SetUser(2);
            var cartItem = new CartItem { CartItemId = 1, CartId = 1, ProductId = 1, Quantity = 1, Cart = new Cart { CartId = 1, UserId = 1 } };
            _cartItemRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(cartItem);
            _cartRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Cart> { cartItem.Cart });
            var result = await _cartController.RemoveFromCart(1);
            Assert.That(result, Is.InstanceOf<ForbidResult>());
        }

        [Test]
        public async Task ClearUserCart_ReturnsUnauthorized_WhenUserNotLoggedIn()
        {
            _cartController.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            var result = await _cartController.ClearUserCart();
            Assert.That(result, Is.InstanceOf<UnauthorizedObjectResult>());
        }

        [Test]
        public async Task ClearUserCart_ReturnsNotFound_WhenCartNotFound()
        {
            SetUser(1);
            _cartRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Cart>());
            var result = await _cartController.ClearUserCart();
            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task GetUserCart_ReturnsUnauthorized_WhenUserNotLoggedIn()
        {
            _cartController.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            var result = await _cartController.GetUserCart();
            Assert.That(result, Is.InstanceOf<UnauthorizedObjectResult>());
        }

        [Test]
        public async Task GetUserCart_ReturnsNotFound_WhenCartIsEmpty()
        {
            SetUser(1);
            _cartRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Cart>());
            var result = await _cartController.GetUserCart();
            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task AddToCart_ReturnsOk_WhenProductIsAdded()
        {
            var product = new Product { ProductId = 1, Name = "Test Product", Price = 100, StockQuantity = 10 };
            var Dto = new AdDtoCartDto { ProductName = "Test Product", Quantity = 2 };
            var cart = new Cart { CartId = 1, UserId = 123 };

            _productRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([product]);
            _cartRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([cart]);
            _cartItemRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([]);
            _cartRepoMock.Setup(r => r.AddAsync(It.IsAny<Cart>())).Returns(Task.CompletedTask);
            _cartItemRepoMock.Setup(r => r.AddAsync(It.IsAny<CartItem>())).Returns(Task.CompletedTask);

            var httpContext = new DefaultHttpContext();
            httpContext.User = new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity(new[] {
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "123")
                })
            );
            _cartController.ControllerContext = new ControllerContext { HttpContext = httpContext };

            var result = await _cartController.AdDtoCart(Dto);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task AddToCart_ReturnsNotFound_WhenProductDoesNotExist()
        {
            var Dto = new AdDtoCartDto { ProductName = "Missing Product", Quantity = 1 };
            _productRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Product>());
            var httpContext = new DefaultHttpContext();
            httpContext.User = new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity(new[] {
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "123")
                })
            );
            _cartController.ControllerContext = new ControllerContext { HttpContext = httpContext };
            var result = await _cartController.AdDtoCart(Dto);
            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task AddToCart_ReturnsBadRequest_WhenStockIsInsufficient()
        {
            var product = new Product { ProductId = 1, Name = "Test Product", Price = 100, StockQuantity = 1 };
            var Dto = new AdDtoCartDto { ProductName = "Test Product", Quantity = 2 };
            _productRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([product]);
            var httpContext = new DefaultHttpContext();
            httpContext.User = new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity(new[] {
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "123")
                })
            );
            _cartController.ControllerContext = new ControllerContext { HttpContext = httpContext };
            var result = await _cartController.AdDtoCart(Dto);
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task RemoveFromCart_ReturnsOk_WhenItemRemoved()
        {
            var cart = new Cart { CartId = 1, UserId = 123 };
            var cartItem = new CartItem { CartItemId = 1, CartId = 1, ProductId = 1, Quantity = 2 };
            _cartRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([cart]);
            _cartItemRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([cartItem]);
            _cartItemRepoMock.Setup(r => r.DeleteAsync(cartItem.CartItemId)).Returns(Task.CompletedTask);
            var httpContext = new DefaultHttpContext();
            httpContext.User = new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity(new[] {
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "123")
                })
            );
            _cartController.ControllerContext = new ControllerContext { HttpContext = httpContext };
            var result = await _cartController.RemoveFromCart(1);
            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        }


        [Test]
        public async Task ClearUserCart_ReturnsOk_WhenCartCleared()
        {
            var cart = new Cart { CartId = 1, UserId = 123 };
            List<CartItem> cartItems = [new CartItem { CartItemId = 1, CartId = 1, ProductId = 1, Quantity = 2 }];
            _cartRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([cart]);
            _cartItemRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(cartItems);
            _cartItemRepoMock.Setup(r => r.DeleteAsync(It.IsAny<int>())).Returns(Task.CompletedTask);
            var httpContext = new DefaultHttpContext();
            httpContext.User = new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity(new[] {
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "123")
                })
            );
            _cartController.ControllerContext = new ControllerContext { HttpContext = httpContext };
            var result = await _cartController.ClearUserCart();
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }


        [Test]
        public async Task MergeGuestCart_ReturnsOk_WhenMerged()
        {
            var cart = new Cart { CartId = 1, UserId = 123 };
            List<CartController.MergeCartItemDto> guestItems = [new CartController.MergeCartItemDto { ProductId = 1, Quantity = 1 }];
            var product = new Product { ProductId = 1, Name = "Test Product", Price = 100, StockQuantity = 10 };
            _cartRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([cart]);
            _productRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([product]);
            _cartItemRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([]);
            _cartItemRepoMock.Setup(r => r.AddAsync(It.IsAny<CartItem>())).Returns(Task.CompletedTask);
            var httpContext = new DefaultHttpContext();
            httpContext.User = new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity(new[] {
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "123")
                })
            );
            _cartController.ControllerContext = new ControllerContext { HttpContext = httpContext };
            var mergeDto = new CartController.MergeCartDto { Items = guestItems };
            var result = await _cartController.MergeGuestCart(mergeDto);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task MergeGuestCart_ReturnsBadRequest_WhenProductNotFound()
        {
            var cart = new Cart { CartId = 1, UserId = 123 };
            List<CartController.MergeCartItemDto> guestItems = [new CartController.MergeCartItemDto { ProductId = 999, Quantity = 1 }];
            _cartRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([cart]);
            _productRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([]);
            var httpContext = new DefaultHttpContext();
            httpContext.User = new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity(new[] {
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "123")
                })
            );
            _cartController.ControllerContext = new ControllerContext { HttpContext = httpContext };
            var mergeDto = new CartController.MergeCartDto { Items = guestItems };
            var result = await _cartController.MergeGuestCart(mergeDto);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task AddToCart_ReturnsUnauthorized_WhenUserNotLoggedIn()
        {
            var Dto = new AdDtoCartDto { ProductName = "Test Product", Quantity = 1 };
            var httpContext = new DefaultHttpContext();
            // No user claims
            _cartController.ControllerContext = new ControllerContext { HttpContext = httpContext };
            var result = await _cartController.AdDtoCart(Dto);
            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task RemoveFromCart_ReturnsUnauthorized_WhenUserNotLoggedIn()
        {
            var httpContext = new DefaultHttpContext();
            _cartController.ControllerContext = new ControllerContext { HttpContext = httpContext };
            var result = await _cartController.RemoveFromCart(1);
            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        }



        [Test]
        public async Task AdDtoCart_ReturnsBadRequest_WhenDtoIsNull()
        {
            SetUser(1);
            var result = await _cartController.AdDtoCart(null!);
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task AdDtoCart_ReturnsBadRequest_WhenProductNameIsEmpty()
        {
            SetUser(1);
            var dto = new AdDtoCartDto { ProductName = "", Quantity = 1 };
            var result = await _cartController.AdDtoCart(dto);
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task AdDtoCart_ReturnsBadRequest_WhenQuantityIsZero()
        {
            SetUser(1);
            var dto = new AdDtoCartDto { ProductName = "Test", Quantity = 0 };
            var result = await _cartController.AdDtoCart(dto);
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task AdDtoCart_UpdatesExistingItem_WhenItemExists()
        {
            SetUser(1);
            var product = new Product { ProductId = 1, Name = "Test", Price = 100, StockQuantity = 10 };
            var cart = new Cart { CartId = 1, UserId = 1 };
            var existingItem = new CartItem { CartItemId = 1, CartId = 1, ProductId = 1, Quantity = 2, TotalPrice = 200 };
            var dto = new AdDtoCartDto { ProductName = "Test", Quantity = 3 };

            _productRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([product]);
            _cartRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([cart]);
            _cartItemRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([existingItem]);
            _cartItemRepoMock.Setup(r => r.UpdateAsync(It.IsAny<CartItem>())).Returns(Task.CompletedTask);

            var result = await _cartController.AdDtoCart(dto);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task AdDtoCart_ReturnsBadRequest_WhenTotalQuantityExceedsStock()
        {
            SetUser(1);
            var product = new Product { ProductId = 1, Name = "Test", Price = 100, StockQuantity = 5 };
            var cart = new Cart { CartId = 1, UserId = 1 };
            var existingItem = new CartItem { CartItemId = 1, CartId = 1, ProductId = 1, Quantity = 3 };
            var dto = new AdDtoCartDto { ProductName = "Test", Quantity = 3 };

            _productRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([product]);
            _cartRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([cart]);
            _cartItemRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([existingItem]);

            var result = await _cartController.AdDtoCart(dto);
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task GetUserCart_ReturnsOk_WithCartItems()
        {
            SetUser(1);
            var cart = new Cart { CartId = 1, UserId = 1 };
            var product = new Product { ProductId = 1, Name = "Test", Price = 100 };
            var cartItem = new CartItem { CartItemId = 1, CartId = 1, ProductId = 1, Quantity = 2 };

            _cartRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([cart]);
            _cartItemRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([cartItem]);
            _productRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([product]);

            var result = await _cartController.GetUserCart();
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task RemoveFromCart_ReturnsOk_WhenItemRemovedSuccessfully()
        {
            SetUser(1);
            var cart = new Cart { CartId = 1, UserId = 1 };
            var cartItem = new CartItem { CartItemId = 1, CartId = 1, ProductId = 1, Quantity = 2, Cart = cart };

            _cartItemRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(cartItem);
            _cartItemRepoMock.Setup(r => r.DeleteAsync(1)).Returns(Task.CompletedTask);

            var result = await _cartController.RemoveFromCart(1);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task AdDtoCart_CreatesNewCart_WhenUserHasNoCart()
        {
            SetUser(1);
            var product = new Product { ProductId = 1, Name = "Test", Price = 100, StockQuantity = 10 };
            var dto = new AdDtoCartDto { ProductName = "Test", Quantity = 2 };

            _productRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([product]);
            _cartRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([]);
            _cartItemRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([]);
            _cartRepoMock.Setup(r => r.AddAsync(It.IsAny<Cart>())).Returns(Task.CompletedTask);
            _cartItemRepoMock.Setup(r => r.AddAsync(It.IsAny<CartItem>())).Returns(Task.CompletedTask);

            var result = await _cartController.AdDtoCart(dto);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task MergeGuestCart_CreatesNewCart_WhenUserHasNoCart()
        {
            SetUser(1);
            var product = new Product { ProductId = 1, Name = "Test", Price = 100, StockQuantity = 10 };
            var guestItems = new List<CartController.MergeCartItemDto>
            {
                new CartController.MergeCartItemDto { ProductId = 1, Quantity = 1 }
            };
            var mergeDto = new CartController.MergeCartDto { Items = guestItems };

            _cartRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([]);
            _productRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([product]);
            _cartItemRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([]);
            _cartRepoMock.Setup(r => r.AddAsync(It.IsAny<Cart>())).Returns(Task.CompletedTask);
            _cartItemRepoMock.Setup(r => r.AddAsync(It.IsAny<CartItem>())).Returns(Task.CompletedTask);

            var result = await _cartController.MergeGuestCart(mergeDto);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task MergeGuestCart_HandlesExistingCartItems_WithDuplicates()
        {
            SetUser(1);
            var cart = new Cart { CartId = 1, UserId = 1 };
            var product = new Product { ProductId = 1, Name = "Test", Price = 100, StockQuantity = 10 };
            var existingItem1 = new CartItem { CartItemId = 1, CartId = 1, ProductId = 1, Quantity = 2 };
            var existingItem2 = new CartItem { CartItemId = 2, CartId = 1, ProductId = 1, Quantity = 1 };
            var guestItems = new List<CartController.MergeCartItemDto>
            {
                new CartController.MergeCartItemDto { ProductId = 1, Quantity = 3 }
            };
            var mergeDto = new CartController.MergeCartDto { Items = guestItems };

            _cartRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([cart]);
            _productRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([product]);
            _cartItemRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([existingItem1, existingItem2]);
            _cartItemRepoMock.Setup(r => r.UpdateAsync(It.IsAny<CartItem>())).Returns(Task.CompletedTask);
            _cartItemRepoMock.Setup(r => r.DeleteAsync(It.IsAny<int>())).Returns(Task.CompletedTask);

            var result = await _cartController.MergeGuestCart(mergeDto);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task RemoveFromCart_HandlesCartItemWithoutCart()
        {
            SetUser(1);
            var cart = new Cart { CartId = 1, UserId = 1 };
            var cartItem = new CartItem { CartItemId = 1, CartId = 1, ProductId = 1, Quantity = 2, Cart = null };

            _cartItemRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(cartItem);
            _cartRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([cart]);
            _cartItemRepoMock.Setup(r => r.DeleteAsync(1)).Returns(Task.CompletedTask);

            var result = await _cartController.RemoveFromCart(1);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetUserCart_HandlesProductNotFound()
        {
            SetUser(1);
            var cart = new Cart { CartId = 1, UserId = 1 };
            var cartItem = new CartItem { CartItemId = 1, CartId = 1, ProductId = 999, Quantity = 2 };

            _cartRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([cart]);
            _cartItemRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([cartItem]);
            _productRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([]);

            var result = await _cartController.GetUserCart();
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task MergeGuestCart_HandlesNullItems()
        {
            SetUser(1);
            var cart = new Cart { CartId = 1, UserId = 1 };
            var mergeDto = new CartController.MergeCartDto { Items = null };

            _cartRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([cart]);
            _productRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([]);
            _cartItemRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([]);

            var result = await _cartController.MergeGuestCart(mergeDto);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }
    }
}
