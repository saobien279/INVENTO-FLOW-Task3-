using FluentValidation;
using InventoFlow.Application.Features.Auth.Commands.Login;

namespace InventoFlow.Application.Features.Auth.Commands.Login
{
    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(v => v.Username).NotEmpty().WithMessage("Vui lòng nhập tài khoản");
            RuleFor(v => v.Password).NotEmpty().WithMessage("Vui lòng nhập mật khẩu");
        }
    }
}
