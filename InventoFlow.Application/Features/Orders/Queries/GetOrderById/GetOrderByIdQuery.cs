using MediatR;
using InventoFlow.Application.DTOs.Order;

namespace InventoFlow.Application.Features.Orders.Queries.GetOrderById
{
    // Query: "Lấy chi tiết 1 đơn hàng theo ID (kèm items)"
    public record GetOrderByIdQuery(int Id) : IRequest<OrderResponseDto?>;
}
