using InventoFlow.Application.DTOs.Category;
using MediatR;

namespace InventoFlow.Application.Features.Categories.Commands.UpdateCategory
{
    public record UpdateCategoryCommand(CategoryUpdateDto Dto) : IRequest<bool>;
}
