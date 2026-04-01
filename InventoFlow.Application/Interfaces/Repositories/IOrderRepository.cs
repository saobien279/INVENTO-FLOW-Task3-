using InventoFlow.Domain.Entities;

namespace InventoFlow.Application.Interfaces.Repositories
{
    public interface IOrderRepository
    {
        // 1. Các hàm cơ bản
        Task<(IEnumerable<Order> Items, int TotalCount)> GetAllAsync(OrderQueryParams query);
        Task AddAsync(Order order);

        // 2. Các hàm bổ sung quan trọng
        Task<Order?> GetByIdAsync(int id);
        Task<IEnumerable<Order>> GetByUserIdAsync(int userId);

        // Hàm bạn đang cảm nhận là cần thiết
        Task<Order?> GetOrderWithDetailsAsync(int id);
    }
}
