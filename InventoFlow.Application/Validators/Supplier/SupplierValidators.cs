using FluentValidation;
using InventoFlow.Application.Features.Suppliers.Commands.CreateSupplier;
using InventoFlow.Application.Features.Suppliers.Commands.UpdateSupplier;
using InventoFlow.Application.Features.Suppliers.Commands.DeleteSupplier;

namespace InventoFlow.Application.Validators.Supplier
{
    public class CreateSupplierCommandValidator : AbstractValidator<CreateSupplierCommand>
    {
        public CreateSupplierCommandValidator()
        {
            RuleFor(x => x.Dto.Name).NotEmpty().WithMessage("Tên nhà cung cấp không được để trống.");
        }
    }

    public class UpdateSupplierCommandValidator : AbstractValidator<UpdateSupplierCommand>
    {
        public UpdateSupplierCommandValidator()
        {
            RuleFor(x => x.Dto.Id).GreaterThan(0).WithMessage("ID không hợp lệ.");
            RuleFor(x => x.Dto.Name).NotEmpty().WithMessage("Tên nhà cung cấp không được để trống.");
        }
    }

    public class DeleteSupplierCommandValidator : AbstractValidator<DeleteSupplierCommand>
    {
        public DeleteSupplierCommandValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0).WithMessage("ID không hợp lệ.");
        }
    }
}
