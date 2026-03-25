using InventoFlow.Infrastructure.Data;
using InventoFlow.Domain.Entities;
using InventoFlow.Application.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace InventoFlow.Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _context;
        public ProductRepository(AppDbContext context) => _context = context;

        public async Task<IEnumerable<Product>> GetAllAsync() => await _context.Products.ToListAsync();
        public async Task<Product?> GetByIdAsync(int id) => await _context.Products.FindAsync(id);
        public async Task<Product?> GetBySKUAsync(string sku) => await _context.Products.FirstOrDefaultAsync(p => p.SKU == sku);
        public async Task AddAsync(Product product) => await _context.Products.AddAsync(product);
        public void Update(Product product) => _context.Products.Update(product);
        public void Delete(Product product) => _context.Products.Remove(product);
        public async Task<bool> SaveChangesAsync() => await _context.SaveChangesAsync() > 0;
    }
}
