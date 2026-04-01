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

        public async Task<(IEnumerable<Product> Items, int TotalCount)> GetAllAsync(ProductQueryParams query)
        {
            var collection = _context.Products.AsQueryable();

            // 1. Searching (Tìm kiếm theo tên hoặc SKU)
            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                var searchTerm = query.SearchTerm.Trim().ToLower();
                collection = collection.Where(p => p.Name.ToLower().Contains(searchTerm) 
                                                || p.SKU.ToLower().Contains(searchTerm));
            }

            // 2. Sorting (Sắp xếp)
            if (!string.IsNullOrWhiteSpace(query.SortBy))
            {
                collection = query.SortBy.ToLower() switch
                {
                    "price_asc" => collection.OrderBy(p => p.Price),
                    "price_desc" => collection.OrderByDescending(p => p.Price),
                    "name_desc" => collection.OrderByDescending(p => p.Name),
                    _ => collection.OrderBy(p => p.Name) // Mặc định theo tên A-Z
                };
            }

            // 3. Đếm tổng số trước khi phân trang
            var totalCount = await collection.CountAsync();

            // 4. Paging (Phân trang)
            // Công thức: Skip = (PageNumber - 1) * PageSize
            var items = await collection
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return (items, totalCount);
        }
        
        public async Task<Product?> GetByIdAsync(int id) => await _context.Products.FindAsync(id);
        public async Task<Product?> GetBySKUAsync(string sku) => await _context.Products.FirstOrDefaultAsync(p => p.SKU == sku);
        public async Task AddAsync(Product product) => await _context.Products.AddAsync(product);
        public void Update(Product product) => _context.Products.Update(product);
        public void Delete(Product product) => _context.Products.Remove(product);
    }
}
