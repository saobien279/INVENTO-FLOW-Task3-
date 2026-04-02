using MediatR;
using AutoMapper;
using InventoFlow.Application.Interfaces.Repositories;
using InventoFlow.Application.DTOs.Order;
using InventoFlow.Application.PageQuery;

namespace InventoFlow.Application.Features.Orders.Queries.GetAllOrders
{
    public class GetAllOrdersHandler : IRequestHandler<GetAllOrdersQuery, PagedResult<OrderResponseDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetAllOrdersHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PagedResult<OrderResponseDto>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
        {
            var (orders, totalCount) = await _unitOfWork.Orders.GetAllAsync(request.Params);

            return new PagedResult<OrderResponseDto>
            {
                Items = _mapper.Map<List<OrderResponseDto>>(orders),
                TotalCount = totalCount,
                PageNumber = request.Params.PageNumber,
                PageSize = request.Params.PageSize
            };
        }
    }
}
