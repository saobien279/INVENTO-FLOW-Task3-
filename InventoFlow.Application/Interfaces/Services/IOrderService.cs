using InventoFlow.Application.DTOs.Order;
using InventoFlow.Application.PageQuery;

namespace InventoFlow.Application.Interfaces.Services
{
    public interface IOrderService
    {
        Task<OrderResponseDto> CreateOrderAsync(OrderCreateDto dto);
        Task<PagedResult<OrderResponseDto>> GetAllOrdersAsync(OrderQueryParams query);
        Task<OrderResponseDto?> GetOrderByIdAsync(int id);
        Task<IEnumerable<OrderResponseDto>> GetOrdersByUserIdAsync(int userId);
    }
}
