namespace InventoFlow.Application.DTOs.Product
{
    public class ProductResponseDto
    {
        public int Id { get; set; } // Phải có dòng này!
        public string Name { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }

        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public int? SupplierId { get; set; }
        public string? SupplierName { get; set; }
    }
}
