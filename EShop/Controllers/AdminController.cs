using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using EShop.Repositories;
using EShop.Models;
using EShop.Dtos;

namespace EShop.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IGenericRepository<Order> _orderRepository;
        private readonly IGenericRepository<OrderItem> _orderItemRepository;
        private readonly IGenericRepository<Payment> _paymentRepository;
        private readonly IGenericRepository<Product> _productRepository;
        private readonly IGenericRepository<User> _userRepository;

        public AdminController(
            IGenericRepository<Order> orderRepository,
            IGenericRepository<OrderItem> orderItemRepository,
            IGenericRepository<Payment> paymentRepository,
            IGenericRepository<Product> productRepository,
            IGenericRepository<User> userRepository)
        {
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _paymentRepository = paymentRepository;
            _productRepository = productRepository;
            _userRepository = userRepository;
        }

        [HttpGet("admin/all-orders")]
        public async Task<IActionResult> GetAllOrders()
        {
            // Log user claims for debugging
            var userClaims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
            Console.WriteLine("User Claims:");
            foreach (var claim in userClaims)
            {
                Console.WriteLine($"Type: {claim.Type}, Value: {claim.Value}");
            }

            // Manually check if the user has the Admin role
            var isAdmin = User.Claims.Any(c => c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" && c.Value == "Admin");
            if (!isAdmin)
            {
                Console.WriteLine("User does not have the Admin role.");
                return Forbid("Bearer");
            }

            var allOrders = (await _orderRepository.GetAllAsync())
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            if (allOrders.Count == 0) return NotFound("No orders found.");

            var allOrderItems = await _orderItemRepository.GetAllAsync();
            var allPayments = await _paymentRepository.GetAllAsync();
            var allProducts = await _productRepository.GetAllAsync();
            var allUsers = await _userRepository.GetAllAsync();

            var result = allOrders.Select(order => new OrderResponseDto
            {
                OrderId = order.OrderId,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                Status = order.Status.ToString(),
                ShippingAddress = order.ShippingAddress,
                PaymentMethod = order.PaymentMethod,
                UserName = allUsers.FirstOrDefault(u => u.UserId == order.UserId)?.FullName,
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
        [HttpPut("admin/update-status/{orderId}")]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, [FromBody] string newStatus)
        {
            var order = (await _orderRepository.GetAllAsync()).FirstOrDefault(o => o.OrderId == orderId);
            if (order == null)
            {
                return NotFound(new { error = $"Order with ID {orderId} not found." });
            }

            if (!Enum.TryParse<OrderStatus>(newStatus, true, out var statusObj))
            {
                return BadRequest(new { error = $"Invalid status value: {newStatus}" });
            }


            // If trying to set status to Delivered, check if payment is done or update COD payment
            if (statusObj == OrderStatus.Delivered)
            {
                var payment = (await _paymentRepository.GetAllAsync()).FirstOrDefault(p => p.OrderId == orderId);
                if (payment == null)
                {
                    return BadRequest(new { error = "Cannot mark as Delivered: Payment is not done." });
                }
                // If payment is COD and not completed, mark as completed
                if (payment.Mode.ToString() == "COD" && payment.Status.ToString() != "Completed")
                {
                    payment.Status = EShop.Models.PaymentStatus.Completed;
                    await _paymentRepository.UpdateAsync(payment);
                }
                else if (payment.Status.ToString() != "Completed")
                {
                    return BadRequest(new { error = "Cannot mark as Delivered: Payment is not done." });
                }
            }

            // If status is being set to Cancelled, refund payment if any
            if (statusObj == OrderStatus.Cancelled)
            {
                var payment = (await _paymentRepository.GetAllAsync()).FirstOrDefault(p => p.OrderId == orderId);
                if (payment != null && payment.Status != EShop.Models.PaymentStatus.Refund)
                {
                    payment.Status = EShop.Models.PaymentStatus.Refund;
                    await _paymentRepository.UpdateAsync(payment);
                }
            }
            order.Status = statusObj;
            await _orderRepository.UpdateAsync(order);

            // Explicitly return JSON with correct content type
            return new JsonResult(new { message = $"Order status updated to {newStatus}" });
        }
        [HttpGet("all-payments")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllPayments()
        {
            // Log user claims for debugging
            var userClaims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
            Console.WriteLine("User Claims:");
            foreach (var claim in userClaims)
            {
                Console.WriteLine($"Type: {claim.Type}, Value: {claim.Value}");
            }

            var payments = (await _paymentRepository.GetAllAsync())
                .Select(p => new PaymentResponseDto
                {
                    PaymentId = p.PaymentId,
                    OrderId = p.OrderId,
                    Mode = p.Mode.ToString(),
                    Amount = p.Amount,
                    PaymentDate = p.PaymentDate,
                    Status = p.Status.ToString()
                }).ToList();

            return Ok(payments);
        }
    }
}
