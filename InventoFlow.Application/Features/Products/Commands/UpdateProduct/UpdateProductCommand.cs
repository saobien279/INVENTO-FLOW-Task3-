using MediatR;
using InventoFlow.Application.DTOs.Product;

namespace InventoFlow.Application.Features.Products.Commands.UpdateProduct
{
    public record UpdateProductCommand(ProductUpdateDto Dto) : IRequest<bool>;
}
