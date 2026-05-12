using InventoFlow.Application.Interfaces.Repositories;
using InventoFlow.Domain.Entities;
using InventoFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoFlow.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
        }

        public async Task<bool> AnyUsernameAsync(string username)
        {
            return await _context.Users.AnyAsync(u => u.Username == username);
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _context.Users
                .Include(u => u.Role) // Bắt buộc phải có dòng này để móc dữ liệu bảng Role lên
                .FirstOrDefaultAsync(u => u.Username == username);
        }
    }
}
