using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using EShop.Models;
using EShop.Repositories;
using EShop.Dtos;
using EShop.Services;

namespace EShop.Controllers
{
    [Authorize(Roles = "Customer")]
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IGenericRepository<Order> _orderRepository;
        private readonly IGenericRepository<OrderItem> _orderItemRepository;
        private readonly IGenericRepository<Cart> _cartRepository;
        private readonly IGenericRepository<CartItem> _cartItemRepository;
        private readonly IGenericRepository<Product> _productRepository;
        private readonly IGenericRepository<Payment> _paymentRepository;
        private readonly IEmailService _emailService;

        public OrderController(
            IGenericRepository<Order> orderRepository,
            IGenericRepository<OrderItem> orderItemRepository,
            IGenericRepository<Cart> cartRepository,
            IGenericRepository<CartItem> cartItemRepository,
            IGenericRepository<Product> productRepository,
            IGenericRepository<Payment> paymentRepository,
            IEmailService emailService)
        {
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _cartRepository = cartRepository;
            _cartItemRepository = cartItemRepository;
            _productRepository = productRepository;
            _paymentRepository = paymentRepository;
            _emailService = emailService;
        }

        [HttpPost("place")]
        public async Task<IActionResult> PlaceOrder([FromBody] OrderRequestDto orderRequest)
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized("User authentication failed.");

            var userCart = (await _cartRepository.GetAllAsync()).FirstOrDefault(c => c.UserId == userId);
            if (userCart == null) return BadRequest("Cart not found for the user.");

            var cartItemsRaw = (await _cartItemRepository.GetAllAsync())
                .Where(ci => ci.CartId == userCart.CartId).ToList();
            if (cartItemsRaw.Count==0) return BadRequest("Cart is empty. Add items before placing an order.");

            var products = await _productRepository.GetAllAsync();

            // Group cart items by ProductId and sum quantities
            var cartItems = cartItemsRaw
                .GroupBy(ci => ci.ProductId)
                .Select(g => new {
                    ProductId = g.Key,
                    Quantity = g.Sum(ci => ci.Quantity),
                    TotalPrice = g.Sum(ci => ci.TotalPrice),
                    CartItemIds = g.Select(ci => ci.CartItemId).ToList()
                }).ToList();

            foreach (var ci in cartItems)
            {
                var product = products.FirstOrDefault(p => p.ProductId == ci.ProductId);
                if (product == null)
                    return BadRequest($"Product with ID {ci.ProductId} not found.");

                if (product.StockQuantity < ci.Quantity)
                    return BadRequest($"Not enough stock for product {product.Name}.");
            }

            var totalAmount = cartItems.Sum(ci => ci.TotalPrice);
            if (!Enum.TryParse<PaymentMethod>(orderRequest.PaymentMethod, true, out var paymentMethod))
            {
                return BadRequest("Invalid payment method. Allowed values: COD, UPI.");
            }

            var newOrder = new Order
            {
                UserId = userId,
                Status = OrderStatus.Pending,
                OrderDate = DateTime.UtcNow,
                TotalAmount = totalAmount,
                ShippingAddress = orderRequest.ShippingAddress,
                PaymentMethod = paymentMethod
            };

            await _orderRepository.AddAsync(newOrder);

            foreach (var ci in cartItems)
            {
                var product = products.First(p => p.ProductId == ci.ProductId);

                var orderItem = new OrderItem
                {
                    OrderId = newOrder.OrderId,
                    ProductId = ci.ProductId,
                    Quantity = ci.Quantity,
                    Price = ci.TotalPrice / ci.Quantity
                };

                await _orderItemRepository.AddAsync(orderItem);

                product.StockQuantity -= ci.Quantity;
                await _productRepository.UpdateAsync(product);

                // Delete all cart items for this product
                foreach (var cartItemId in ci.CartItemIds)
                {
                    await _cartItemRepository.DeleteAsync(cartItemId);
                }
            }

            string userEmail = GetCurrentUserEmail();

            // Build product details for email
            var orderItemDetails = cartItems.Select(ci =>
            {
                var product = products.First(p => p.ProductId == ci.ProductId);
                return $"- {product.Name} (Qty: {ci.Quantity}, Price: ₹{ci.TotalPrice})";
            });
            string orderDetails = string.Join("\n", orderItemDetails);

            string emailBody =
                $"Your order #{newOrder.OrderId} has been placed successfully!\n\n" +
                $"Order Summary:\n" +
                $"{orderDetails}\n\n" +
                $"Shipping Address: {newOrder.ShippingAddress}\n" +
                $"Total Amount: ₹{newOrder.TotalAmount}\n" +
                $"Payment Method: {newOrder.PaymentMethod}";

            await _emailService.SendEmailAsync(
                toEmail: userEmail,
                subject: "Order Placed Successfully",
                body: emailBody
            );

            var orderItems = (await _orderItemRepository.GetAllAsync())
                .Where(oi => oi.OrderId == newOrder.OrderId)
                .Select(oi => new OrderItemResponseDto
                {
                    OrderItemId = oi.OrderItemId,
                    ProductId = oi.ProductId,
                    Quantity = oi.Quantity,
                    Price = oi.Price
                }).ToList();

            var response = new OrderResponseDto
            {
                OrderId = newOrder.OrderId,
                OrderDate = newOrder.OrderDate,
                TotalAmount = newOrder.TotalAmount,
                Status = newOrder.Status.ToString(),
                ShippingAddress = newOrder.ShippingAddress,
                PaymentMethod = newOrder.PaymentMethod,
                Items = orderItems,
                Payment = null
            };

            return Ok(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null) return NotFound("Order not found.");

            int userId = GetCurrentUserId();
            if (userId == 0 || order.UserId != userId)
                return Forbid("Unauthorized to cancel this order.");

            if (order.Status == OrderStatus.Shipped || order.Status == OrderStatus.Delivered)
                return BadRequest("Order cannot be canceled after it has been shipped.");

            var orderItems = (await _orderItemRepository.GetAllAsync()).Where(oi => oi.OrderId == id).ToList();
            var allProducts = await _productRepository.GetAllAsync();

            foreach (var item in orderItems)
            {
                var product = allProducts.FirstOrDefault(p => p.ProductId == item.ProductId);
                if (product != null)
                {
                    product.StockQuantity += item.Quantity;
                    await _productRepository.UpdateAsync(product);
                }

                await _orderItemRepository.DeleteAsync(item.OrderItemId);
            }

            order.Status = OrderStatus.Cancelled;
            await _orderRepository.UpdateAsync(order);

            var payment = (await _paymentRepository.GetAllAsync())
                .FirstOrDefault(p => p.OrderId == order.OrderId);

            string userEmail = GetCurrentUserEmail();
            string emailBody = $"Your order #{order.OrderId} has been cancelled.";

            if (payment != null)
            {
                payment.Status = EShop.Models.PaymentStatus.Refund;
                await _paymentRepository.UpdateAsync(payment);
                emailBody += $"\nRefund of ₹{payment.Amount} will be processed to your {(payment.Mode == PaymentMode.UPI ? "UPI account" : "COD method")} within 5–7 business days.";
            }

            await _emailService.SendEmailAsync(
                toEmail: userEmail,
                subject: "Order Cancelled",
                body: emailBody
            );

            return Ok("Order canceled successfully.");
        }

        [HttpGet]
        public async Task<IActionResult> GetMyOrders()
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized("User authentication failed.");

            var myOrders = (await _orderRepository.GetAllAsync())
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            if (myOrders.Count == 0) return NotFound("No orders found.");

            var allOrderItems = await _orderItemRepository.GetAllAsync();
            var allPayments = await _paymentRepository.GetAllAsync();
            var allProducts = await _productRepository.GetAllAsync();

            var result = myOrders.Select(order => new OrderResponseDto
            {
                OrderId = order.OrderId,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                Status = order.Status.ToString(),
                ShippingAddress = order.ShippingAddress,
                PaymentMethod = order.PaymentMethod,
                Items = allOrderItems
                    .Where(oi => oi.OrderId == order.OrderId)
                    .Select(oi => new OrderItemResponseDto
                    {
                        OrderItemId = oi.OrderItemId,
                        ProductId = oi.ProductId,
                        ProductName = allProducts.FirstOrDefault(p => p.ProductId == oi.ProductId)?.Name,
                        Quantity = oi.Quantity,
                        Price = oi.Price
                    }).ToList(),
                Payment = allPayments
                    .Where(p => p.OrderId == order.OrderId)
                    .Select(p => new PaymentResponseDto
                    {
                        PaymentId = p.PaymentId,
                        OrderId = p.OrderId,
                        Mode = p.Mode.ToString(),
                        Amount = p.Amount,
                        PaymentDate = p.PaymentDate,
                        Status = p.Status.ToString()
                    }).FirstOrDefault()
            });

            return Ok(result);
        }

        
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        private string GetCurrentUserEmail()
        {
            return User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
        }
    }
}
