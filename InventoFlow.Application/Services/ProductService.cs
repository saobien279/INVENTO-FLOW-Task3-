using AutoMapper;
using InventoFlow.Application.DTOs.Product;
using InventoFlow.Application.PageQuery;
using InventoFlow.Domain.Entities;
using InventoFlow.Application.Interfaces.Repositories;
using InventoFlow.Application.Interfaces.Services;

namespace InventoFlow.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProductService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ProductResponseDto> CreateProductAsync(ProductCreateDto dto)
        {
            // Dùng AutoMapper để chuyển DTO thành Model Product
            var product = _mapper.Map<Product>(dto);

           // Gọi qua UoW
            await _unitOfWork.Products.AddAsync(product);
            // Chốt hạ bằng UoW
            await _unitOfWork.CompleteAsync();
            // Chuyển Model vừa tạo thành Response DTO để trả về
            return _mapper.Map<ProductResponseDto>(product);
        }

        public async Task<PagedResult<ProductResponseDto>> GetAllProductsAsync(ProductQueryParams query)
        {
            var (products, totalCount) = await _unitOfWork.Products.GetAllAsync(query);
    
            return new PagedResult<ProductResponseDto>
            {
                Items = _mapper.Map<List<ProductResponseDto>>(products),
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }

        public async Task<ProductResponseDto?> GetProductByIdAsync(int id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            return _mapper.Map<ProductResponseDto>(product);
        }

        public async Task<bool> UpdateProductAsync(int id, ProductUpdateDto dto)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null) return false;

            // Cập nhật thông tin từ DTO vào Model đã tìm thấy
            _mapper.Map(dto, product);
            _unitOfWork.Products.Update(product);
            return await _unitOfWork.CompleteAsync() > 0;
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null) return false;

            _unitOfWork.Products.Delete(product);
            return await _unitOfWork.CompleteAsync() > 0;
        }
    }
}