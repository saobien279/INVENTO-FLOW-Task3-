using InventoFlow.Application.DTOs.Category;
using MediatR;

namespace InventoFlow.Application.Features.Categories.Queries.GetCategoryById
{
    public record GetCategoryByIdQuery(int Id) : IRequest<CategoryResponseDto?>;
}
