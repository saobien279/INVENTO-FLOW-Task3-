using InventoFlow.Application.Interfaces.Repositories;
using InventoFlow.Infrastructure.Data;

namespace InventoFlow.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        
        // Khai báo các Repo thực thi
        public IProductRepository Products { get; private set; }
        public IOrderRepository Orders { get; private set; }
        public IUserRepository Users { get; private set; }

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            
            // Khởi tạo các Repo và truyền chung 1 context vào
            Products = new ProductRepository(_context);
            Orders = new OrderRepository(_context);
            Users = new UserRepository(_context);
        }   

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        // Triển khai thêm Transaction nếu cần can thiệp sâu
        public async Task BeginTransactionAsync() => await _context.Database.BeginTransactionAsync();
        public async Task CommitTransactionAsync() => await _context.Database.CurrentTransaction!.CommitAsync();
        public async Task RollbackTransactionAsync() => await _context.Database.CurrentTransaction!.RollbackAsync();
    }
}