using FluentValidation;
using InventoFlow.Application.Features.Products.Commands.CreateProduct;

namespace InventoFlow.Application.Features.Products.Commands.CreateProduct
{
    public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
    {
        public CreateProductCommandValidator()
        {
            RuleFor(v => v.Name)
                .NotEmpty().WithMessage("Tên sản phẩm không được để trống")
                .MaximumLength(200).WithMessage("Tên không được quá 200 ký tự");

            RuleFor(v => v.Price)
                .GreaterThan(0).WithMessage("Giá sản phẩm phải lớn hơn 0");

            RuleFor(v => v.SKU)
                .NotEmpty().WithMessage("Mã SKU là bắt buộc");
        }
    }
}