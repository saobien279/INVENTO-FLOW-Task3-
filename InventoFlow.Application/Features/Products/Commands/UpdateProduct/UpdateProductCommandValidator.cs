using FluentValidation;
using InventoFlow.Application.Features.Products.Commands.UpdateProduct;

namespace InventoFlow.Application.Features.Products.Commands.UpdateProduct
{
    public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
    {
        public UpdateProductCommandValidator()
        {
            RuleFor(v => v.Dto.Id).GreaterThan(0).WithMessage("ID sản phẩm không hợp lệ");
            RuleFor(v => v.Dto.Name).NotEmpty().MaximumLength(200);
            RuleFor(v => v.Dto.Price).GreaterThan(0).WithMessage("Giá phải lớn hơn 0");
            RuleFor(v => v.Dto.StockQuantity).GreaterThanOrEqualTo(0).WithMessage("Số lượng kho không được âm");
        }
    }
}
