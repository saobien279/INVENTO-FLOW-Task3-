using AutoMapper;
using InventoFlow.Application.DTOs.Product;
using InventoFlow.Domain.Entities;
using InventoFlow.Application.Interfaces.Repositories;
using InventoFlow.Application.Interfaces.Services;

namespace InventoFlow.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepo;
        private readonly IMapper _mapper;

        public ProductService(IProductRepository productRepo, IMapper mapper)
        {
            _productRepo = productRepo;
            _mapper = mapper;
        }

        public async Task<ProductResponseDto> CreateProductAsync(ProductCreateDto dto)
        {
            // Dùng AutoMapper để chuyển DTO thành Model Product
            var product = _mapper.Map<Product>(dto);

            await _productRepo.AddAsync(product);
            await _productRepo.SaveChangesAsync();

            // Chuyển Model vừa tạo thành Response DTO để trả về
            return _mapper.Map<ProductResponseDto>(product);
        }

        public async Task<IEnumerable<ProductResponseDto>> GetAllProductsAsync()
        {
            var products = await _productRepo.GetAllAsync();
            return _mapper.Map<IEnumerable<ProductResponseDto>>(products);
        }

        public async Task<ProductResponseDto?> GetProductByIdAsync(int id)
        {
            var product = await _productRepo.GetByIdAsync(id);
            return _mapper.Map<ProductResponseDto>(product);
        }

        public async Task<bool> UpdateProductAsync(int id, ProductUpdateDto dto)
        {
            var product = await _productRepo.GetByIdAsync(id);
            if (product == null) return false;

            // Cập nhật thông tin từ DTO vào Model đã tìm thấy
            _mapper.Map(dto, product);
            _productRepo.Update(product);
            return await _productRepo.SaveChangesAsync();
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _productRepo.GetByIdAsync(id);
            if (product == null) return false;

            _productRepo.Delete(product);
            return await _productRepo.SaveChangesAsync();
        }
    }
}