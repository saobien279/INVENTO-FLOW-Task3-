using FluentValidation;
using InventoFlow.Application.Features.Products.Commands.CreateProduct;

namespace InventoFlow.Application.Features.Products.Commands.CreateProduct
{
    public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
    {
        public CreateProductCommandValidator()
        {
            RuleFor(v => v.Dto.Name)
                .NotEmpty().WithMessage("Tên sản phẩm không được để trống")
                .MaximumLength(200).WithMessage("Tên không được quá 200 ký tự");

            RuleFor(v => v.Dto.Price)
                .GreaterThan(0).WithMessage("Giá sản phẩm phải lớn hơn 0");

            RuleFor(v => v.Dto.SKU)
                .NotEmpty().WithMessage("Mã SKU là bắt buộc");

            RuleFor(v => v.Dto.StockQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("Số lượng kho không được âm");
        }
    }
}