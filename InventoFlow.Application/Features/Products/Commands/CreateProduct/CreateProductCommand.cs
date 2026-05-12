using MediatR;
using InventoFlow.Application.DTOs.Product;

namespace InventoFlow.Application.Features.Products.Commands.CreateProduct
{
    public record CreateProductCommand(ProductCreateDto Dto) : IRequest<ProductResponseDto>;
}