using FluentValidation;
using InventoFlow.Application.Features.Categories.Commands.UpdateCategory;

namespace InventoFlow.Application.Validators.Category
{
    public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
    {
        public UpdateCategoryCommandValidator()
        {
            RuleFor(x => x.Dto.Id).GreaterThan(0).WithMessage("ID danh mục không hợp lệ.");
            RuleFor(x => x.Dto.Name).NotEmpty().WithMessage("Tên danh mục không được để trống.");
            
            // Validate Logic nâng cao: Danh mục cha không thể trỏ vào chính nó
            RuleFor(x => x.Dto)
                .Must(dto => dto.ParentId != dto.Id)
                .WithMessage("Danh mục cha không thể là chính nó.");
        }
    }
}
