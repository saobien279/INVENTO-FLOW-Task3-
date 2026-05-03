using Microsoft.AspNetCore.Mvc;
using MediatR;
using InventoFlow.Application.DTOs.Category;
using InventoFlow.Application.Features.Categories.Queries.GetAllCategories;
using InventoFlow.Application.Features.Categories.Commands.CreateCategory;

namespace INVENTO_FLOW.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CategoriesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _mediator.Send(new GetAllCategoriesQuery());
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CategoryCreateDto dto)
        {
            var command = new CreateCategoryCommand { Dto = dto };
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}
