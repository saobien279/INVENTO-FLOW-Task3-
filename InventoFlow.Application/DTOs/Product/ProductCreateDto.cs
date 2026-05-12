using System.ComponentModel.DataAnnotations;

namespace InventoFlow.Application.DTOs.Product
{
    public class ProductCreateDto
    {
        
        public string Name { get; set; } = string.Empty;

        public string SKU { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public int StockQuantity { get; set; }

        public int? CategoryId { get; set; }
        public int? SupplierId { get; set; }
    }
}
