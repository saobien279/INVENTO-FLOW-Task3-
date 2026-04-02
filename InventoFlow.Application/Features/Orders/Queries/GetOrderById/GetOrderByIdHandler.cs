using MediatR;
using AutoMapper;
using InventoFlow.Application.Interfaces.Repositories;
using InventoFlow.Application.DTOs.Order;

namespace InventoFlow.Application.Features.Orders.Queries.GetOrderById
{
    public class GetOrderByIdHandler : IRequestHandler<GetOrderByIdQuery, OrderResponseDto?>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetOrderByIdHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<OrderResponseDto?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
        {
            // Dùng GetOrderWithDetailsAsync để Include đủ OrderItems + Product
            var order = await _unitOfWork.Orders.GetOrderWithDetailsAsync(request.Id);
            if (order == null) return null;

            return _mapper.Map<OrderResponseDto>(order);
        }
    }
}
