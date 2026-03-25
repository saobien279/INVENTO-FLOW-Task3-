namespace InventoFlow.Application.DTOs.Order
{
    public class OrderItemResponseDto
    {
        public int ProductId { get; set; }
        public string? ProductName { get; set; } // Trả về tên để hiển thị lên giao diện
        public int Quantity { get; set; }
        public decimal PriceAtPurchase { get; set; }
        public decimal SubTotal => Quantity * PriceAtPurchase; // Tự tính thành tiền của món hàng
    }
}
    