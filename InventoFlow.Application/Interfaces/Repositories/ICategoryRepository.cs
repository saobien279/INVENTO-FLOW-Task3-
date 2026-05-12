using InventoFlow.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InventoFlow.Application.Interfaces.Repositories
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetAllAsync();
        Task<Category?> GetByIdAsync(int id);
        Task AddAsync(Category category);
        void Update(Category category);
        void Delete(Category category);
        Task<bool> AnyNameAsync(string name); // Kiểm tra trùng tên danh mục
    }
}
