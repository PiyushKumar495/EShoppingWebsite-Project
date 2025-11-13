using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EShop.Models
{
    public enum OrderStatus
    {
        Pending,
        Shipped,
        Delivered,
        Cancelled
    }
    public enum PaymentMethod
    {
        COD,
        UPI
    }

    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        public OrderStatus Status { get; set; }

        [Required]
        [MaxLength(500)]
        public string? ShippingAddress { get; set; }
        [Required]
        public PaymentMethod PaymentMethod { get; set; }
        public virtual Payment? Payment { get; set; }

        public virtual ICollection<OrderItem> Items { get; set; } = [];
    }
}
