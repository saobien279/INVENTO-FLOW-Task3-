using InventoFlow.Application.DTOs.Supplier;
using InventoFlow.Application.Features.Suppliers.Commands.CreateSupplier;
using InventoFlow.Application.Features.Suppliers.Commands.DeleteSupplier;
using InventoFlow.Application.Features.Suppliers.Commands.UpdateSupplier;
using InventoFlow.Application.Features.Suppliers.Queries.GetAllSuppliers;
using InventoFlow.Application.Features.Suppliers.Queries.GetSupplierById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace INVENTO_FLOW.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SuppliersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SuppliersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _mediator.Send(new GetAllSuppliersQuery());
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _mediator.Send(new GetSupplierByIdQuery(id));
            if (result == null) return NotFound("Không tìm thấy nhà cung cấp.");
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SupplierCreateDto dto)
        {
            var result = await _mediator.Send(new CreateSupplierCommand(dto));
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] SupplierUpdateDto dto)
        {
            if (id != dto.Id) return BadRequest("ID không khớp.");
            var success = await _mediator.Send(new UpdateSupplierCommand(dto));
            if (!success) return NotFound("Không tìm thấy nhà cung cấp.");
            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _mediator.Send(new DeleteSupplierCommand(id));
            if (!success) return BadRequest("Xóa thất bại. Nhà cung cấp không tồn tại.");
            return NoContent();
        }
    }
}
