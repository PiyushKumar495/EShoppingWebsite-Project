using EShop.Models;

namespace EShop.Dtos
{
    public class OrderResponseDto
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Status { get; set; }
        public string? ShippingAddress { get; set; }
        public string? UserName { get; set; } // Added for display
        public List<OrderItemResponseDto>? Items { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public PaymentResponseDto? Payment { get; set; }
    }
}
