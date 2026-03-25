using System.ComponentModel.DataAnnotations;

namespace INVENTO_FLOW.DTOs.Order
{
    public class OrderCreateDto
    {
        [Required]
        public int UserId { get; set; } // ID của người đặt hàng

        [Required]
        [MinLength(1, ErrorMessage = "Đơn hàng phải có ít nhất một sản phẩm.")]
        public List<OrderItemCreateDto> Items { get; set; } = new List<OrderItemCreateDto>();
    }
}
