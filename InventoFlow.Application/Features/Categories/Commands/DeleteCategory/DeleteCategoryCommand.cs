using MediatR;

namespace InventoFlow.Application.Features.Categories.Commands.DeleteCategory
{
    public record DeleteCategoryCommand(int Id) : IRequest<bool>;
}
