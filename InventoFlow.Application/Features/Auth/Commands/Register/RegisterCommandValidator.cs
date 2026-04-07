using FluentValidation;
using InventoFlow.Application.Features.Auth.Commands.Register;

namespace InventoFlow.Application.Features.Auth.Commands.Register
{
    public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
    {
        public RegisterCommandValidator()
        {
            RuleFor(v => v.Username)
                .NotEmpty().WithMessage("Tên đăng nhập không được để trống")
                .MinimumLength(3).WithMessage("Tên đăng nhập phải có ít nhất 3 ký tự");

            RuleFor(v => v.Password)
                .NotEmpty().WithMessage("Mật khẩu không được để trống")
                .MinimumLength(6).WithMessage("Mật khẩu phải từ 6 ký tự trở lên");
        }
    }
}
