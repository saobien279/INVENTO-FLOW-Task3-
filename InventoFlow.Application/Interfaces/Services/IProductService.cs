using InventoFlow.Application.DTOs.Product;

namespace InventoFlow.Application.Interfaces.Services
{
    public interface IProductService
    {
        Task<ProductResponseDto> CreateProductAsync(ProductCreateDto dto);
        Task<IEnumerable<ProductResponseDto>> GetAllProductsAsync();
        Task<ProductResponseDto?> GetProductByIdAsync(int id);
        Task<bool> UpdateProductAsync(int id, ProductUpdateDto dto);
        Task<bool> DeleteProductAsync(int id);
    }
}
