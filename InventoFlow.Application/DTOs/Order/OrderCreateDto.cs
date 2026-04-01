using System.ComponentModel.DataAnnotations;

namespace InventoFlow.Application.DTOs.Order
{
    public class OrderCreateDto
    {
        public int UserId { get; set; } // ID của người đặt hàng

        public List<OrderItemCreateDto> Items { get; set; } = new List<OrderItemCreateDto>();
    }
}
