using InventoFlow.Application.DTOs.Product;
using InventoFlow.Application.PageQuery;

namespace InventoFlow.Application.Interfaces.Services
{
    public interface IProductService
    {
        Task<ProductResponseDto> CreateProductAsync(ProductCreateDto dto);
        Task<PagedResult<ProductResponseDto>> GetAllProductsAsync(ProductQueryParams query);
        Task<ProductResponseDto?> GetProductByIdAsync(int id);
        Task<bool> UpdateProductAsync(int id, ProductUpdateDto dto);
        Task<bool> DeleteProductAsync(int id);
    }
}
