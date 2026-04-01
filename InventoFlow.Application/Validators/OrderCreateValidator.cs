
using FluentValidation;
using InventoFlow.Application.DTOs.Order;

public class OrderCreateValidator : AbstractValidator<OrderCreateDto>
{
    public OrderCreateValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0).WithMessage("Người dùng không hợp lệ.");

        // Kiểm tra danh sách không được trống
        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Đơn hàng phải có ít nhất 1 sản phẩm.");

        // Kiểm tra từng món hàng bên trong danh sách (RuleForEach)
        RuleForEach(x => x.Items).ChildRules(item => {
            item.RuleFor(i => i.ProductId).GreaterThan(0).WithMessage("ID sản phẩm không hợp lệ.");;
            item.RuleFor(i => i.Quantity).GreaterThan(0).WithMessage("Số lượng phải > 0.");
        });
    }
}