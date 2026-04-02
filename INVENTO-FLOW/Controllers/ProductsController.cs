using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using InventoFlow.Application.Features.Products.Queries.GetAllProducts;
using InventoFlow.Application.Features.Products.Queries.GetProductById;
using InventoFlow.Application.Features.Products.Commands.CreateProduct;
using InventoFlow.Application.Features.Products.Commands.UpdateProduct;
using InventoFlow.Application.Features.Products.Commands.DeleteProduct;
using InventoFlow.Application.PageQuery;

namespace INVENTO_FLOW.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IMediator _mediator;

        // Constructor Injection: chỉ cần IMediator, không cần bất kỳ Service nào khác
        public ProductsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // 1. Lấy danh sách tất cả sản phẩm (có phân trang, tìm kiếm, sort)
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] ProductQueryParams query)
        {
            var result = await _mediator.Send(new GetAllProductsQuery(query));
            return Ok(result);
        }

        // 2. Lấy thông tin chi tiết 1 sản phẩm theo Id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _mediator.Send(new GetProductByIdQuery(id));
            return Ok(result);
        }

        // 3. Thêm mới sản phẩm — Chỉ Admin
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductCommand command)
        {
            var result = await _mediator.Send(command);
            // 201 Created + Header Location + toàn bộ dữ liệu sản phẩm mới
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        // 4. Cập nhật sản phẩm — Chỉ Admin
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProductCommand command)
        {
            // Đảm bảo Id trong route khớp với Id trong body
            if (id != command.Id)
                return BadRequest(new { message = "Id trên route và Id trong body không khớp." });

            var success = await _mediator.Send(command);
            if (!success)
                return NotFound(new { message = "Cập nhật thất bại. Sản phẩm không tồn tại." });

            return NoContent();
        }

        // 5. Xóa sản phẩm — Chỉ Admin
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _mediator.Send(new DeleteProductCommand(id));
            if (!success)
                return NotFound(new { message = "Xóa thất bại. Sản phẩm không tồn tại." });

            return NoContent();
        }
    }
}