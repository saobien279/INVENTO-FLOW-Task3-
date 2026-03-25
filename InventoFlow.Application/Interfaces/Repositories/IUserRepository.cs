using InventoFlow.Domain.Entities;

namespace InventoFlow.Application.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<bool> AnyUsernameAsync(string username);
        Task<User?> GetByUsernameAsync(string username);
        Task AddAsync(User user);
        Task<int> SaveChangesAsync();
    }
}
