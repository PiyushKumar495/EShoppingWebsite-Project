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
    [Authorize(Roles = "Customer")]
    public class UserController : ControllerBase
    {
        private readonly IGenericRepository<Order> _orderRepository;
        private readonly IGenericRepository<OrderItem> _orderItemRepository;
        private readonly IGenericRepository<Payment> _paymentRepository;

        public UserController(
            IGenericRepository<Order> orderRepository,
            IGenericRepository<OrderItem> orderItemRepository,
            IGenericRepository<Payment> paymentRepository)
        {
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _paymentRepository = paymentRepository;
        }

        [HttpGet("orders")]
        public async Task<IActionResult> GetUserOrders()
        {
            int userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized("User authentication failed.");

            var orders = (await _orderRepository.GetAllAsync())
                         .Where(o => o.UserId == userId)
                         .ToList();

            if (orders.Count==0)
                return NotFound("No orders found for the user.");

            var allOrderItems = await _orderItemRepository.GetAllAsync();
            var allPayments = await _paymentRepository.GetAllAsync();

            var result = orders.Select(order => new OrderResponseDto
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

        [HttpGet("orders/payments")]
        public async Task<IActionResult> GetUserPayments()
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized("User authentication failed.");

            var orders = (await _orderRepository.GetAllAsync())
                         .Where(o => o.UserId == userId)
                         .ToList();

            var orderIds = orders.Select(o => o.OrderId).ToList();
            var payments = (await _paymentRepository.GetAllAsync())
                            .Where(p => orderIds.Contains(p.OrderId))
                            .Select(p => new PaymentResponseDto
                            {
                                PaymentId = p.PaymentId,
                                OrderId = p.OrderId,
                                Mode = p.Mode.ToString(),
                                Amount = p.Amount,
                                PaymentDate = p.PaymentDate,
                                Status = p.Status.ToString()
                            }).ToList();

            if (payments.Count==0)
                return NotFound("No payment records found.");

            return Ok(payments);
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }
    }
}
