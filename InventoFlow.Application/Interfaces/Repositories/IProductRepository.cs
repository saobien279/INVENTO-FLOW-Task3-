using InventoFlow.Domain.Entities;


namespace InventoFlow.Application.Interfaces.Repositories
{
    public interface IProductRepository
    {
        Task<(IEnumerable<Product> Items, int TotalCount)> GetAllAsync(ProductQueryParams query);
        Task<Product?> GetByIdAsync(int id);
        Task<Product?> GetBySKUAsync(string sku);
        Task AddAsync(Product product);
        void Update(Product product);
        void Delete(Product product);
    }
}
