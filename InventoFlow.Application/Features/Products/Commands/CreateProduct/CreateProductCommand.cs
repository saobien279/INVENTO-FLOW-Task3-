using MediatR;
using InventoFlow.Application.DTOs.Product;

namespace InventoFlow.Application.Features.Products.Commands.CreateProduct
{
    // Command: "Lệnh tạo sản phẩm mới, trả về ProductResponseDto đầy đủ thông tin"
    public record CreateProductCommand(
        string Name,
        string SKU,
        decimal Price,
        int StockQuantity
    ) : IRequest<ProductResponseDto>;
}