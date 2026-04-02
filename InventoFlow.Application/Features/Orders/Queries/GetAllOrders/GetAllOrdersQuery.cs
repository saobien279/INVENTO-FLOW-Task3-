using MediatR;
using InventoFlow.Application.DTOs.Order;
using InventoFlow.Application.PageQuery;

namespace InventoFlow.Application.Features.Orders.Queries.GetAllOrders
{
    // Query: "Lấy tất cả đơn hàng (Admin), có phân trang + filter theo UserId"
    public record GetAllOrdersQuery(OrderQueryParams Params) : IRequest<PagedResult<OrderResponseDto>>;
}
