using FluentValidation;
using InventoFlow.Application.Features.Orders.Commands.CreateOrder;

namespace InventoFlow.Application.Features.Orders.Commands.CreateOrder
{
    public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
    {
        public CreateOrderCommandValidator()
        {
            RuleFor(v => v.UserId).GreaterThan(0).WithMessage("Người dùng không hợp lệ");
            
            RuleFor(v => v.Items)
                .NotEmpty().WithMessage("Đơn hàng phải có ít nhất 1 sản phẩm");

            // Kiểm tra từng món hàng trong danh sách
            RuleForEach(v => v.Items).ChildRules(item => {
                item.RuleFor(i => i.ProductId).GreaterThan(0).WithMessage("ID sản phẩm không đúng");
                item.RuleFor(i => i.Quantity).GreaterThan(0).WithMessage("Số lượng mua phải ít nhất là 1");
            });
        }
    }
}
