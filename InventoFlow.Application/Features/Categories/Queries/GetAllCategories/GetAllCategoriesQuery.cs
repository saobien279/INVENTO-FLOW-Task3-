using InventoFlow.Application.DTOs.Category;
using MediatR;
using System.Collections.Generic;

namespace InventoFlow.Application.Features.Categories.Queries.GetAllCategories
{
    public class GetAllCategoriesQuery : IRequest<IEnumerable<CategoryResponseDto>> { }
}
