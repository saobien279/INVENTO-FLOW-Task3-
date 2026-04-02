using MediatR;

namespace InventoFlow.Application.Features.Products.Commands.DeleteProduct
{
    // Request: "Tôi muốn xóa Sản phẩm theo ID, trả về true nếu thành công, false nếu không thấy"
    public record DeleteProductCommand(int Id) : IRequest<bool>;
}
