using MediatR;
using AutoMapper;
using InventoFlow.Application.Interfaces.Repositories;
using InventoFlow.Application.DTOs.Product;
using InventoFlow.Domain.Entities;

namespace InventoFlow.Application.Features.Products.Commands.CreateProduct
{
    public class CreateProductHandler : IRequestHandler<CreateProductCommand, ProductResponseDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CreateProductHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ProductResponseDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            // 1. Tạo entity Product từ dữ liệu Command
            var product = new Product
            {
                Name = request.Name,
                SKU = request.SKU,
                Price = request.Price,
                StockQuantity = request.StockQuantity
            };

            // 2. Thêm vào Change Tracker của EF Core
            await _unitOfWork.Products.AddAsync(product);

            // 3. Lưu vào Database (product.Id sẽ được DB gán tự động sau bước này)
            await _unitOfWork.CompleteAsync();

            // 4. Trả về toàn bộ thông tin sản phẩm (kể cả Id mới)
            return _mapper.Map<ProductResponseDto>(product);
        }
    }
}