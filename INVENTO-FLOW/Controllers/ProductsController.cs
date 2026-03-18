using INVENTO_FLOW.DTOs.Product;
using INVENTO_FLOW.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace INVENTO_FLOW.Controllers
{
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
        public async Task<ActionResult<ProductResponseDto>> Create(ProductCreateDto dto)
        {
            // ASP.NET Core sẽ tự động validate dựa trên các [Attribute] Sen đặt ở DTO
            var result = await _productService.CreateProductAsync(dto);

            // Trả về 201 Created cùng link dẫn đến sản phẩm vừa tạo
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        // 4. Cập nhật thông tin sản phẩm
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(ProductUpdateDto dto)
        {
            var success = await _productService.UpdateProductAsync(dto.Id, dto);
            if (!success)
            {
                return NotFound(new { message = "Cập nhật thất bại. Sản phẩm không tồn tại." });
            }
            return NoContent(); // Trả về 204 nếu thành công
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