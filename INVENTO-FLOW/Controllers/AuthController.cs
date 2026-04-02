using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using InventoFlow.Application.Features.Auth.Commands.Register;
using InventoFlow.Application.Features.Auth.Commands.Login;

namespace INVENTO_FLOW.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // Đăng ký tài khoản mới (không cần xác thực)
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterCommand command)
        {
            var success = await _mediator.Send(command);
            if (!success)
                return BadRequest(new { message = "Username đã tồn tại." });

            return Ok(new { message = "Đăng ký thành công." });
        }

        // Đăng nhập — trả về JWT token (không cần xác thực)
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginCommand command)
        {
            var token = await _mediator.Send(command);
            if (token == null)
                return Unauthorized(new { message = "Sai tài khoản hoặc mật khẩu." });

            return Ok(new { Token = token });
        }
    }
}