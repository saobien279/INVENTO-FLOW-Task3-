using FluentValidation;
using InventoFlow.Application.Features.Categories.Commands.DeleteCategory;

namespace InventoFlow.Application.Validators.Category
{
    public class DeleteCategoryCommandValidator : AbstractValidator<DeleteCategoryCommand>
    {
        public DeleteCategoryCommandValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0).WithMessage("ID danh mục không hợp lệ.");
        }
    }
}
