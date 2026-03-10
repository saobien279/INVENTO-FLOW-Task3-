namespace INVENTO_FLOW.DTOs.Product
{
    public class ProductResponseDto
    {
        public int Id { get; set; } // Phải có dòng này!
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
    }
}
