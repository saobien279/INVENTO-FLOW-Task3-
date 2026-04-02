using MediatR;
using InventoFlow.Application.DTOs.Order;

namespace InventoFlow.Application.Features.Orders.Queries.GetOrdersByUserId
{
    // Query: "Lấy tất cả đơn hàng của một User (kèm items)"
    public record GetOrdersByUserIdQuery(int UserId) : IRequest<IEnumerable<OrderResponseDto>>;
}
