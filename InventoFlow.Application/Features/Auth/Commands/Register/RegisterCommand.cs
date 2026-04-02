using MediatR;

namespace InventoFlow.Application.Features.Auth.Commands.Register
{
    // Command: "Lệnh đăng ký tài khoản mới, trả về true nếu thành công"
    public record RegisterCommand(string Username, string Password) : IRequest<bool>;
}
