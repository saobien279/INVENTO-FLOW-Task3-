using INVENTO_FLOW.DTOs.Order;
using INVENTO_FLOW.Services.Interfaces;
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

        // 1. Tạo đơn hàng mới (Đặt hàng)
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