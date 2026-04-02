using MediatR;
using InventoFlow.Application.DTOs.Product;

namespace InventoFlow.Application.Features.Products.Commands.UpdateProduct
{
    // Command: "Lệnh cập nhật sản phẩm, trả về true nếu thành công"
    public record UpdateProductCommand(
        int Id,
        string? Name,
        string? SKU,
        decimal Price,
        int StockQuantity
    ) : IRequest<bool>;
}
