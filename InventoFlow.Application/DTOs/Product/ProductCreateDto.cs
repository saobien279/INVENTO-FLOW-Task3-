using System.ComponentModel.DataAnnotations;

namespace InventoFlow.Application.DTOs.Product
{
    public class ProductCreateDto
    {
        [Required(ErrorMessage = "Tên sản phẩm là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên sản phẩm không được quá 100 ký tự")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mã SKU là bắt buộc")]
        [RegularExpression(@"^[A-Z0-9-]*$", ErrorMessage = "SKU chỉ được chứa chữ hoa, số và dấu gạch ngang")]
        public string SKU { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "Giá sản phẩm phải lớn hơn hoặc bằng 0")]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Số lượng tồn kho không được âm")]
        public int StockQuantity { get; set; }
    }
}
