using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using InventoFlow.Application.Features.Orders.Commands.CreateOrder;
using InventoFlow.Application.Features.Orders.Queries.GetAllOrders;
using InventoFlow.Application.Features.Orders.Queries.GetOrderById;
using InventoFlow.Application.Features.Orders.Queries.GetOrdersByUserId;
using InventoFlow.Application.DTOs.Order;
using InventoFlow.Application.PageQuery;

namespace INVENTO_FLOW.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OrdersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // 1. Lấy tất cả đơn hàng — Chỉ Admin, có phân trang + filter
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] OrderQueryParams query)
        {
            var result = await _mediator.Send(new GetAllOrdersQuery(query));
            return Ok(result);
        }

        // 2. Tạo đơn hàng mới (đặt hàng)
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOrderCommand command)
        {
            var result = await _mediator.Send(command);
            // 201 Created + Location header trỏ tới GetById
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        // 3. Lấy chi tiết 1 đơn hàng theo Id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _mediator.Send(new GetOrderByIdQuery(id));
            if (result == null)
                return NotFound(new { message = $"Không tìm thấy đơn hàng có ID {id}." });

            return Ok(result);
        }

        // 4. Lấy lịch sử đơn hàng của 1 User
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUserId(int userId)
        {
            var result = await _mediator.Send(new GetOrdersByUserIdQuery(userId));
            return Ok(result);
        }
    }
}