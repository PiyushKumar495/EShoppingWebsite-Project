using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using EShop.Models;
using EShop.Dtos;
using EShop.Repositories;

namespace EShop.Controllers
{
    [Authorize(Roles = "Customer")]
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        private const string AuthFailedMessage = "User authentication failed.";
        private readonly IGenericRepository<Cart> _cartRepository;
        private readonly IGenericRepository<CartItem> _cartItemRepository;
        private readonly IGenericRepository<Product> _productRepository;

        public CartController(
        IGenericRepository<Cart> cartRepository,
        IGenericRepository<CartItem> cartItemRepository,
        IGenericRepository<Product> productRepository)
        {
            _cartRepository = cartRepository;
            _cartItemRepository = cartItemRepository;
            _productRepository = productRepository;
        }
        private async Task MergeOrAddCartItem(List<CartItem> userCartItems, MergeCartItemDto item, List<Product> allProducts, int userCartId)
        {
            var product = allProducts.FirstOrDefault(p => p.ProductId == item.ProductId);
            if (product == null) return;
            var existingItems = userCartItems.Where(ci => ci.ProductId == item.ProductId).ToList();
            if (existingItems.Count > 0)
            {
                var mainItem = existingItems[0];
                mainItem.Quantity += item.Quantity;
                mainItem.TotalPrice = mainItem.Quantity * product.Price;
                await _cartItemRepository.UpdateAsync(mainItem);
                foreach (var dup in existingItems.Skip(1))
                {
                    await _cartItemRepository.DeleteAsync(dup.CartItemId);
                }
            }
            else
            {
                var newCartItem = new CartItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    CartId = userCartId,
                    TotalPrice = item.Quantity * product.Price
                };
                await _cartItemRepository.AddAsync(newCartItem);
            }
        }

        // Helper: Remove duplicate items for products not in guest cart
        private async Task RemoveDuplicateCartItems(List<CartItem> userCartItems)
        {
            var duplicates = userCartItems
                .GroupBy(ci => ci.ProductId)
                .SelectMany(g => g.Skip(1));

            foreach (var dup in duplicates)
            {
                await _cartItemRepository.DeleteAsync(dup.CartItemId);
            }
        }

        [HttpPost("add")]
        public async Task<IActionResult> AdDtoCart([FromBody] AdDtoCartDto Dto)
        {
            if (Dto == null || string.IsNullOrWhiteSpace(Dto.ProductName) || Dto.Quantity <= 0)
                return BadRequest("Invalid data. Product name and quantity are required.");

            var product = (await _productRepository.GetAllAsync())
                .FirstOrDefault(p => p.Name?.Equals(Dto.ProductName, StringComparison.OrdinalIgnoreCase) == true);

            if (product == null)
                return NotFound("Product not found.");

            if (product.StockQuantity < Dto.Quantity)
                return BadRequest("Not enough stock available.");

            int userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(AuthFailedMessage);

            var userCart = (await _cartRepository.GetAllAsync())
                .FirstOrDefault(c => c.UserId == userId) ?? new Cart { UserId = userId };

            if (userCart.CartId == 0)
                await _cartRepository.AddAsync(userCart);

            var existingItem = (await _cartItemRepository.GetAllAsync())
                .FirstOrDefault(ci => ci.CartId == userCart.CartId && ci.ProductId == product.ProductId);

            int totalRequested = Dto.Quantity + (existingItem?.Quantity ?? 0);
            if (product.StockQuantity < totalRequested)
                return BadRequest("Not enough stock available.");

            if (existingItem != null)
            {
                existingItem.Quantity += Dto.Quantity;
                existingItem.TotalPrice = existingItem.Quantity * product.Price;
                await _cartItemRepository.UpdateAsync(existingItem);
            }
            else
            {
                var newCartItem = new CartItem
                {
                    ProductId = product.ProductId,
                    Product = product,
                    Quantity = Dto.Quantity,
                    CartId = userCart.CartId,
                    Cart = userCart,
                    TotalPrice = Dto.Quantity * product.Price
                };
                await _cartItemRepository.AddAsync(newCartItem);
            }

            return Ok(new { Message = "Item added to cart successfully." });
        }
        [HttpPost("merge")]
        public async Task<IActionResult> MergeGuestCart([FromBody] MergeCartDto Dto)
        {
            int userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(AuthFailedMessage);

            var userCart = (await _cartRepository.GetAllAsync()).FirstOrDefault(c => c.UserId == userId);
            if (userCart == null)
            {
                userCart = new Cart { UserId = userId };
                await _cartRepository.AddAsync(userCart);
            }

            var allProducts = (await _productRepository.GetAllAsync()).ToList();
            var guestItems = Dto.Items ?? new List<MergeCartItemDto>();
            
            foreach (var item in guestItems)
            {
                // Get fresh cart items for each iteration to include newly added items
                var allCartItems = await _cartItemRepository.GetAllAsync();
                var userCartItems = allCartItems.Where(ci => ci.CartId == userCart.CartId).ToList();
                await MergeOrAddCartItem(userCartItems, item, allProducts, userCart.CartId);
            }
            
            // Clean up any remaining duplicates
            var finalCartItems = await _cartItemRepository.GetAllAsync();
            var finalUserCartItems = finalCartItems.Where(ci => ci.CartId == userCart.CartId).ToList();
            await RemoveDuplicateCartItems(finalUserCartItems);
            
            return Ok(new { message = "Cart merged successfully." });
        }


        // Place these Dtos at the end of the controller for clarity and make them private
        public class MergeCartDto
        {
            public List<MergeCartItemDto>? Items { get; set; }
        }
        public class MergeCartItemDto
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; }
        }

        [HttpDelete("item/{id}")]
        public async Task<IActionResult> RemoveFromCart(int id)
        {
            var cartItem = await _cartItemRepository.GetByIdAsync(id);
            if (cartItem == null)
                return NotFound("Cart item not found.");

            int userId = GetCurrentUserId();
            // Ensure cart is loaded for the item
            Cart? cart = null;
            if (cartItem.Cart != null)
            {
                cart = cartItem.Cart;
            }
            else
            {
                // Try to fetch cart by CartId
                cart = (await _cartRepository.GetAllAsync()).FirstOrDefault(c => c.CartId == cartItem.CartId);
            }
            if (userId == 0 || cart == null || cart.UserId != userId)
                return Forbid();

            await _cartItemRepository.DeleteAsync(id);
            return Ok(new { Message = "Item removed from cart." });
        }

        [HttpDelete("clear")]
        public async Task<IActionResult> ClearUserCart()
        {
            int userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(AuthFailedMessage);

            var userCart = (await _cartRepository.GetAllAsync()).FirstOrDefault(c => c.UserId == userId);
            if (userCart == null)
                return NotFound("No cart found.");

            var cartItems = (await _cartItemRepository.GetAllAsync())
                .Where(ci => ci.CartId == userCart.CartId).ToList();

            foreach (var item in cartItems)
            {
                await _cartItemRepository.DeleteAsync(item.CartItemId);
            }

            return Ok(new { message = "Cart cleared." });
        }

        [HttpGet]
        public async Task<IActionResult> GetUserCart()
        {
            int userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(AuthFailedMessage);

            var userCart = (await _cartRepository.GetAllAsync()).FirstOrDefault(c => c.UserId == userId);
            if (userCart == null)
                return NotFound("Cart is empty.");

            var allItems = await _cartItemRepository.GetAllAsync();
            var cartItems = allItems.Where(ci => ci.CartId == userCart.CartId).ToList();

            var allProducts = await _productRepository.GetAllAsync();

            var responseItems = new List<CartItemResponseDto>();
            decimal granDtotal = 0;

            foreach (var item in cartItems)
            {
                var product = allProducts.FirstOrDefault(p => p.ProductId == item.ProductId);
                if (product != null)
                {
                    var itemTotal = product.Price * item.Quantity;
                    granDtotal += itemTotal;

                    responseItems.Add(new CartItemResponseDto
                    {
                        CartItemId = item.CartItemId,
                        ProductId = item.ProductId,
                        ProductName = product.Name,
                        Price = product.Price,
                        Quantity = item.Quantity,
                        TotalPrice = itemTotal
                    });
                }
            }

            var cartResponse = new CartResponseDto
            {
                CartId = userCart.CartId,
                Items = responseItems,
                GranDtotal = granDtotal
            };

            return Ok(cartResponse);
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c =>
                c.Type == ClaimTypes.NameIdentifier || c.Type.EndsWith("nameidentifier"));

            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
        }
    }
}