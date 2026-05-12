using InventoFlow.Application.DTOs.Category;
using MediatR;

namespace InventoFlow.Application.Features.Categories.Commands.CreateCategory
{
    public class CreateCategoryCommand : IRequest<CategoryResponseDto>
    {
        public CategoryCreateDto Dto { get; }
        public CreateCategoryCommand(CategoryCreateDto dto) => Dto = dto;
    }
}
