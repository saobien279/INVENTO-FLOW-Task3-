namespace INVENTO_FLOW.DTOs.Order
{
    public class OrderItemCreateDto
    {
        public int ProductId { get; set; } // ID sản phẩm muốn mua
        public int Quantity { get; set; }  // Số lượng mua
    }
}
