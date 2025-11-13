namespace EShop.Dtos
{
    public class CartResponseDto
    {
        public int CartId { get; set; }
        public List<CartItemResponseDto> Items { get; set; } = new List<CartItemResponseDto>();

        public decimal GranDtotal { get; set; }
    }
}
