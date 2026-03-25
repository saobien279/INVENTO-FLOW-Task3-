using InventoFlow.Application.DTOs.Order;
using InventoFlow.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace INVENTO_FLOW.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // 1. Lấy tất cả đơn hàng (Chỉ Admin mới thấy được toàn bộ)
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderResponseDto>>> GetAll()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return Ok(orders);
        }

        // 2. Tạo đơn hàng mới (Đặt hàng)
        [HttpPost]
        public async Task<ActionResult<OrderResponseDto>> Create([FromBody] OrderCreateDto dto)
        {
            try
            {
                var result = await _orderService.CreateOrderAsync(dto);
                // Trả về 201 Created và thông tin đơn hàng vừa tạo
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                var innerError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return BadRequest(new { message = innerError });
            }
        }

        // 2. Lấy thông tin chi tiết 1 đơn hàng
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderResponseDto>> GetById(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                return NotFound(new { message = $"Không tìm thấy đơn hàng {id}" });
            }
            return Ok(order);
        }

        // 3. Lấy lịch sử đơn hàng của 1 User
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<OrderResponseDto>>> GetByUserId(int userId)
        {
            var orders = await _orderService.GetOrdersByUserIdAsync(userId);
            return Ok(orders);
        }
    }
}