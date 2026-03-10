namespace INVENTO_FLOW.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty; // Mã hàng duy nhất
        public decimal Price { get; set; }
        public int StockQuantity { get; set; } // Số lượng trong kho
    }
}
