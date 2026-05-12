using InventoFlow.Application.Interfaces.Repositories;
using InventoFlow.Domain.Entities;
using InventoFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InventoFlow.Infrastructure.Repositories
{
    public class SupplierRepository : ISupplierRepository
    {
        private readonly AppDbContext _context;

        public SupplierRepository(AppDbContext context) => _context = context;

        public async Task<IEnumerable<Supplier>> GetAllAsync() => await _context.Suppliers.ToListAsync();
        
        public async Task<Supplier?> GetByIdAsync(int id) => await _context.Suppliers.FirstOrDefaultAsync(s => s.Id == id);
        
        public async Task AddAsync(Supplier supplier) => await _context.Suppliers.AddAsync(supplier);
        
        public void Update(Supplier supplier) => _context.Suppliers.Update(supplier);
        
        public void Delete(Supplier supplier) => _context.Suppliers.Remove(supplier);
    }
}
