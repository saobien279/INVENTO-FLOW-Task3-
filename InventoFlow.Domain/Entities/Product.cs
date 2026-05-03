namespace InventoFlow.Domain.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty; // Mã hàng duy nhất
        public decimal Price { get; set; }
        public int StockQuantity { get; set; } // Số lượng trong kho
        
        // --- CÁC KHOÁ NGOẠI MỚI THÊM ---
        public int? CategoryId { get; set; }
        public Category? Category { get; set; }

        public int? SupplierId { get; set; }
        public Supplier? Supplier { get; set; }


    }
}
