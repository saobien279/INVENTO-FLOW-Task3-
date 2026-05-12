using FluentValidation;
using InventoFlow.Application.Features.Categories.Commands.CreateCategory;

namespace InventoFlow.Application.Validators.Category
{
    public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
    {
        public CreateCategoryCommandValidator()
        {
            RuleFor(x => x.Dto.Name).NotEmpty().WithMessage("Tên danh mục không được để trống.");
        }
    }
}
