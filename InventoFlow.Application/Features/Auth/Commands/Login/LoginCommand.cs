using MediatR;

namespace InventoFlow.Application.Features.Auth.Commands.Login
{
    // Command: "Lệnh đăng nhập, trả về JWT token (null nếu sai thông tin)"
    public record LoginCommand(string Username, string Password) : IRequest<string?>;
}
