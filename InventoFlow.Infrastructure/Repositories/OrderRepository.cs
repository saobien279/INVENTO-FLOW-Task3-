using InventoFlow.Infrastructure.Data;
using InventoFlow.Domain.Entities;
using InventoFlow.Application.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace InventoFlow.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _context;

        public OrderRepository(AppDbContext context)
        {
            _context = context;
        }

        // 1. Lấy tất cả đơn hàng (kèm chi tiết OrderItems và Product)
        public async Task<IEnumerable<Order>> GetAllAsync()
        {
            return await _context.Orders
                .Include(o => o.OrderItems!)
                .ThenInclude(oi => oi.Product)
                .ToListAsync();
        }

        // 2. Lấy đơn hàng theo ID (không kèm chi tiết món hàng)
        public async Task<Order?> GetByIdAsync(int id)
        {
            return await _context.Orders.FindAsync(id);
        }

        // 3. Lấy đơn hàng kèm theo chi tiết các món hàng (OrderItems) và thông tin Sản phẩm
        public async Task<Order?> GetOrderWithDetailsAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.OrderItems!)          // Lấy danh sách các món hàng
                .ThenInclude(oi => oi.Product)      // Lấy thông tin sản phẩm của từng món hàng
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        // 4. Lấy danh sách đơn hàng của một người dùng cụ thể (kèm chi tiết)
        public async Task<IEnumerable<Order>> GetByUserIdAsync(int userId)
        {
            return await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderItems!)
                .ThenInclude(oi => oi.Product)
                .ToListAsync();
        }

        // 5. Thêm đơn hàng mới
        public async Task AddAsync(Order order)
        {
            // Khi thêm Order có chứa List<OrderItem>, EF sẽ tự động thêm vào cả 2 bảng
            await _context.Orders.AddAsync(order);
        }

        // 6. Lưu thay đổi xuống Database
        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}