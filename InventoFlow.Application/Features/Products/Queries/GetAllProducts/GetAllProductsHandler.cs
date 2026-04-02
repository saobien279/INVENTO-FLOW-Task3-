using MediatR;
using AutoMapper;
using InventoFlow.Application.Interfaces.Repositories;
using InventoFlow.Application.DTOs.Product;
using InventoFlow.Application.PageQuery;

namespace InventoFlow.Application.Features.Products.Queries.GetAllProducts
{
    public class GetAllProductsHandler : IRequestHandler<GetAllProductsQuery, PagedResult<ProductResponseDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetAllProductsHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PagedResult<ProductResponseDto>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
        {
            // Gọi repository để lấy danh sách + tổng số bản ghi
            var (products, totalCount) = await _unitOfWork.Products.GetAllAsync(request.Params);

            return new PagedResult<ProductResponseDto>
            {
                Items = _mapper.Map<List<ProductResponseDto>>(products),
                TotalCount = totalCount,
                PageNumber = request.Params.PageNumber,
                PageSize = request.Params.PageSize
            };
        }
    }
}
