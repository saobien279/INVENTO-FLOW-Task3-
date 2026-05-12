using InventoFlow.Application.DTOs.Category;
using InventoFlow.Application.Features.Categories.Commands.CreateCategory;
using InventoFlow.Application.Features.Categories.Commands.DeleteCategory;
using InventoFlow.Application.Features.Categories.Commands.UpdateCategory;
using InventoFlow.Application.Features.Categories.Queries.GetAllCategories;
using InventoFlow.Application.Features.Categories.Queries.GetCategoryById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace INVENTO_FLOW.Controllers
{
    [Authorize]
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _mediator.Send(new GetCategoryByIdQuery(id));
            if (result == null) return NotFound("Không tìm thấy danh mục.");
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CategoryCreateDto dto)
        {
            var result = await _mediator.Send(new CreateCategoryCommand(dto));
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CategoryUpdateDto dto)
        {
            if (id != dto.Id) return BadRequest("ID không khớp.");
            var success = await _mediator.Send(new UpdateCategoryCommand(dto));
            if (!success) return NotFound("Không tìm thấy danh mục.");
            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _mediator.Send(new DeleteCategoryCommand(id));
            if (!success) return BadRequest("Xóa thất bại. Danh mục không tồn tại hoặc đang có danh mục con.");
            return NoContent();
        }
    }
}
