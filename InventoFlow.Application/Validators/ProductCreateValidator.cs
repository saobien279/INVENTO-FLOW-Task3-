using FluentValidation;
using InventoFlow.Application.DTOs.Product;

namespace InventoFlow.Application.Validators
{
    public class ProductCreateValidator : AbstractValidator<ProductCreateDto>
    {
        public ProductCreateValidator()
        {
            // Tên sản phẩm không được để trống và tối đa 100 ký tự
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Tên sản phẩm là bắt buộc.")
                .MaximumLength(100).WithMessage("Tên không được quá 100 ký tự.");

            // Giá phải lớn hơn 0
            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Giá sản phẩm phải lớn hơn 0.");

            // Số lượng tồn kho không được là số âm
            RuleFor(x => x.StockQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("Số lượng kho không được âm.");
        }
    }
}