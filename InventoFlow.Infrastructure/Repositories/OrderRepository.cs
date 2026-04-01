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

        // 1. Lấy tất cả đơn hàng có hỗ trợ Filter / Sort / Paging
        public async Task<(IEnumerable<Order> Items, int TotalCount)> GetAllAsync(OrderQueryParams query)
        {
            var collection = _context.Orders
                .Include(o => o.OrderItems!)
                .ThenInclude(oi => oi.Product)
                .AsQueryable();

            // 1. Filter theo UserId (nếu có)
            if (query.UserId.HasValue)
            {
                collection = collection.Where(o => o.UserId == query.UserId.Value);
            }

            // 2. Sorting
            if (!string.IsNullOrWhiteSpace(query.SortBy))
            {
                collection = query.SortBy.ToLower() switch
                {
                    "date_asc"    => collection.OrderBy(o => o.OrderDate),
                    "date_desc"   => collection.OrderByDescending(o => o.OrderDate),
                    "amount_asc"  => collection.OrderBy(o => o.TotalAmount),
                    "amount_desc" => collection.OrderByDescending(o => o.TotalAmount),
                    _             => collection.OrderByDescending(o => o.OrderDate) // Mặc định mới nhất trước
                };
            }
            else
            {
                collection = collection.OrderByDescending(o => o.OrderDate);
            }

            // 3. Đếm tổng số trước khi phân trang
            var totalCount = await collection.CountAsync();

            // 4. Phân trang
            var items = await collection
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return (items, totalCount);
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
            await _context.Orders.AddAsync(order);
        }
    }
}