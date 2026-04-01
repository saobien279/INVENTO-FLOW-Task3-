namespace InventoFlow.Application.Interfaces.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        // Khai báo các Repository để Service có thể gọi thông qua UoW
        IProductRepository Products { get; }
        IOrderRepository Orders { get; }
        IUserRepository Users { get; }

        // Hàm quan trọng nhất: Lưu tất cả thay đổi trong một Transaction
        Task<int> CompleteAsync();
        
        // Các hàm bổ trợ cho Transaction thủ công (nếu cần logic cực khó)
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}