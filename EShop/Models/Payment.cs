using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EShop.Models
{
    public enum PaymentStatus
    {
        Pending,
        Completed,
        Failed,
        Refund
    }

    public enum PaymentMode
    {
        COD,
        UPI,
        
    }

    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }

        [Required]
        public int OrderId { get; set; }

        [ForeignKey("OrderId")]
        public Order? Order { get; set; }

        [Required]
        public PaymentMode Mode { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        [Required]
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    }
}
