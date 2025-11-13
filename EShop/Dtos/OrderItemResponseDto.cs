namespace EShop.Dtos
{
    public class OrderItemResponseDto
    {
        public int OrderItemId { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; } // Added for display
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
