using FluentValidation;
using InventoFlow.Application.Features.Products.Commands.UpdateProduct;

namespace InventoFlow.Application.Features.Products.Commands.UpdateProduct
{
    public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
    {
        public UpdateProductCommandValidator()
        {
            RuleFor(v => v.Id).GreaterThan(0).WithMessage("ID sản phẩm không hợp lệ");
            RuleFor(v => v.Name).NotEmpty().MaximumLength(200);
            RuleFor(v => v.Price).GreaterThan(0).WithMessage("Giá phải lớn hơn 0");
            RuleFor(v => v.StockQuantity).GreaterThanOrEqualTo(0).WithMessage("Số lượng kho không được âm");
        }
    }
}
