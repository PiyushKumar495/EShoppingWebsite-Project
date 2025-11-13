namespace EShop.Dtos
{
    public class OrderRequestDto
    {
        public string? ShippingAddress { get; set; }
        public string? PaymentMethod { get; set; }
    }
}
