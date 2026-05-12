using InventoFlow.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InventoFlow.Application.Interfaces.Repositories
{
    public interface ISupplierRepository
    {
        Task<IEnumerable<Supplier>> GetAllAsync();
        Task<Supplier?> GetByIdAsync(int id);
        Task AddAsync(Supplier supplier);
        void Update(Supplier supplier);
        void Delete(Supplier supplier);
    }
}
