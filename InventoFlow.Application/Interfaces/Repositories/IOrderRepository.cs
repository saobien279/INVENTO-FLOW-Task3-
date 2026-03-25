using InventoFlow.Domain.Entities;

namespace InventoFlow.Application.Interfaces.Repositories
{
    public interface IOrderRepository
    {
        // 1. Các hàm cơ bản bạn đã đoán
        Task<IEnumerable<Order>> GetAllAsync();
        Task AddAsync(Order order);
        Task<bool> SaveChangesAsync();

        // 2. Các hàm bổ sung quan trọng
        Task<Order?> GetByIdAsync(int id);
        Task<IEnumerable<Order>> GetByUserIdAsync(int userId);

        // Hàm bạn đang cảm nhận là cần thiết
        Task<Order?> GetOrderWithDetailsAsync(int id);
    }
}
