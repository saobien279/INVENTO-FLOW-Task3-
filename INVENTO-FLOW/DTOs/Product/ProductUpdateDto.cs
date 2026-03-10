namespace INVENTO_FLOW.DTOs.Product
{
    public class ProductUpdateDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? SKU { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }

        // Sen có thể thêm logic hiển thị trạng thái kho tại đây
        public string StockStatus => StockQuantity > 0 ? "Còn hàng" : "Hết hàng";
    }
}
