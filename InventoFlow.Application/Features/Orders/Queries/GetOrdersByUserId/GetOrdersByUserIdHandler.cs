using MediatR;
using AutoMapper;
using InventoFlow.Application.Interfaces.Repositories;
using InventoFlow.Application.DTOs.Order;

namespace InventoFlow.Application.Features.Orders.Queries.GetOrdersByUserId
{
    public class GetOrdersByUserIdHandler : IRequestHandler<GetOrdersByUserIdQuery, IEnumerable<OrderResponseDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetOrdersByUserIdHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<OrderResponseDto>> Handle(GetOrdersByUserIdQuery request, CancellationToken cancellationToken)
        {
            // GetByUserIdAsync đã Include OrderItems + Product sẵn trong Repository
            var orders = await _unitOfWork.Orders.GetByUserIdAsync(request.UserId);
            return _mapper.Map<IEnumerable<OrderResponseDto>>(orders);
        }
    }
}
