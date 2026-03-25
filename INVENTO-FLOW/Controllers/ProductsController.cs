using INVENTO_FLOW.DTOs.Product;
using INVENTO_FLOW.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace INVENTO_FLOW.Controllers
{
    // Pseudocode plan:
    // 1. Check for possible issues in the controller code.
    // 2. Validate route and method signatures match REST conventions and DTO usage.
    // 3. Ensure model binding is correct for PUT/POST methods (use [FromBody] where needed).
    // 4. Check for missing or incorrect attributes.
    // 5. Ensure correct usage of CreatedAtAction and NotFound responses.
    // 6. Check for possible null reference or validation issues.

    // Issues found and fixes:
    // - The UpdateProduct method should accept the id from the route and the DTO from the body, not just the DTO (to match REST conventions and avoid mismatches).
    // - Add [FromBody] to POST and PUT methods for clarity and to ensure correct model binding.
    // - The UpdateProduct method should have the signature: public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductUpdateDto dto)
    // - In UpdateProduct, ensure the id from the route matches dto.Id, or set dto.Id = id for consistency.

    // Fixed controller code:
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        // 1. Lấy danh sách tất cả sản phẩm
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductResponseDto>>> GetAll()
        {
            var products = await _productService.GetAllProductsAsync();
            return Ok(products);
        }

        // 2. Lấy thông tin chi tiết 1 sản phẩm theo Id
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductResponseDto>> GetById(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound(new { message = $"Không tìm thấy sản phẩm có Id {id}" });
            }
            return Ok(product);
        }

        // 3. Thêm mới sản phẩm (Nhập kho)
        [HttpPost]
        public async Task<ActionResult<ProductResponseDto>> Create([FromBody] ProductCreateDto dto)
        {
            var result = await _productService.CreateProductAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        // 4. Cập nhật thông tin sản phẩm
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductUpdateDto dto)
        {
            if (id != dto.Id)
            {
                return BadRequest(new { message = "Id trên route và Id trong dữ liệu không khớp." });
            }
            var success = await _productService.UpdateProductAsync(id, dto);
            if (!success)
            {
                return NotFound(new { message = "Cập nhật thất bại. Sản phẩm không tồn tại." });
            }
            return NoContent();
        }

        // 5. Xóa sản phẩm
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _productService.DeleteProductAsync(id);
            if (!success)
            {
                return NotFound(new { message = "Xóa thất bại. Sản phẩm không tồn tại." });
            }
            return NoContent();
        }
    }
}