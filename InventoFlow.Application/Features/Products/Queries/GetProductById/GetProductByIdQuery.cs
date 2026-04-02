using MediatR;
using InventoFlow.Application.DTOs.Product;

namespace InventoFlow.Application.Features.Products.Queries.GetProductById
{
    // Request: "Tôi muốn lấy Product theo ID, kết quả trả về là ProductResponseDto"
    public record GetProductByIdQuery(int Id) : IRequest<ProductResponseDto>;
}