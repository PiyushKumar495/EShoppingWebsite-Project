using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EShop.Models;
using EShop.Dtos;
using EShop.Repositories;
using EShop.Services;
using System.Security.Claims;

namespace EShop.Controllers
{
    [Authorize(Roles = "Customer")]
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IGenericRepository<Payment> _paymentRepository;
        private readonly IGenericRepository<Order> _orderRepository;
        private readonly IEmailService _emailService;

        public PaymentController(
            IGenericRepository<Payment> paymentRepository,
            IGenericRepository<Order> orderRepository,
            IEmailService emailService)
        {
            _paymentRepository = paymentRepository;
            _orderRepository = orderRepository;
            _emailService = emailService;
        }

        [HttpPost]
        public async Task<IActionResult> MakePayment([FromBody] PaymentCreateDto Dto)
        {
            var order = await _orderRepository.GetByIdAsync(Dto.OrderId);
            if (order == null)
                return NotFound("Order not found.");

            if (Dto.Amount == 0)
            {
                return Ok(new { Message = "Please enter the amount equal to the total order amount.", TotalAmount = order.TotalAmount });
            }

            if (Dto.Amount != order.TotalAmount)
                return BadRequest("Please enter the exact amount equal to the total order amount.");

            var existing = (await _paymentRepository.GetAllAsync())
                .FirstOrDefault(p => p.OrderId == Dto.OrderId);

            if (existing != null)
                return BadRequest("Payment already exists for this order.");

            if (!Enum.TryParse<PaymentMode>(Dto.Mode, true, out var mode))
                return BadRequest("Invalid payment mode.");

            if (!string.Equals(order.PaymentMethod.ToString(), Dto.Mode, StringComparison.OrdinalIgnoreCase))
                return BadRequest("Payment mode does not match the selected method on the order.");

            var payment = new Payment
            {
                OrderId = Dto.OrderId,
                Mode = mode,
                Amount = Dto.Amount ?? 0,
                Status = mode == PaymentMode.COD ? PaymentStatus.Pending : PaymentStatus.Completed
            };

            await _paymentRepository.AddAsync(payment);

            // Send invoice if UPI
            if (mode == PaymentMode.UPI)
            {
                string userEmail = GetCurrentUserEmail();
                string invoice = $"Thank you for your payment!\n\n" +
                                 $"Payment ID: {payment.PaymentId}\n" +
                                 $"Order ID: {payment.OrderId}\n" +
                                 $"Amount Paid: ₹{payment.Amount}\n" +
                                 $"Payment Mode: {payment.Mode}\n" +
                                 $"Date: {payment.PaymentDate:yyyy-MM-dd HH:mm:ss}\n" +
                                 $"Status: {payment.Status}";

                await _emailService.SendEmailAsync(
                    toEmail: userEmail,
                    subject: "Payment Invoice - EShop",
                    body: invoice
                );
            }

            return Ok(new PaymentResponseDto
            {
                PaymentId = payment.PaymentId,
                OrderId = payment.OrderId,
                Mode = payment.Mode.ToString(),
                Amount = payment.Amount,
                PaymentDate = payment.PaymentDate,
                Status = payment.Status.ToString()
            });
        }

        

        private string GetCurrentUserEmail()
        {
            return User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
        }
    }
}
