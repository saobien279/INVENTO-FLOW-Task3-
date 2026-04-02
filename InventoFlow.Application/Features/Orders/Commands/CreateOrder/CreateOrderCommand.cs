using MediatR;
using InventoFlow.Application.DTOs.Order;

namespace InventoFlow.Application.Features.Orders.Commands.CreateOrder
{
    // Command: "Lệnh tạo đơn hàng mới, trả về OrderResponseDto đầy đủ"
    public record CreateOrderCommand(
        int UserId,
        List<OrderItemCreateDto> Items
    ) : IRequest<OrderResponseDto>;
}
