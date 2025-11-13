using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
namespace EShop.Dtos
{
    public class PaymentCreateDto
    {
        [BindRequired]
        public int OrderId { get; set; }
        public string? Mode { get; set; } // COD, UPI, Card, etc.
        [Required]
        public decimal? Amount { get; set; }
    }
}
