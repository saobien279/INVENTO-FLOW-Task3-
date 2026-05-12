using InventoFlow.Application.DTOs.Supplier;
using InventoFlow.Application.Interfaces.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace InventoFlow.Application.Features.Suppliers.Commands.UpdateSupplier
{
    public record UpdateSupplierCommand(SupplierUpdateDto Dto) : IRequest<bool>;

    public class UpdateSupplierHandler : IRequestHandler<UpdateSupplierCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateSupplierHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(UpdateSupplierCommand request, CancellationToken cancellationToken)
        {
            var supplier = await _unitOfWork.Suppliers.GetByIdAsync(request.Dto.Id);
            if (supplier == null) return false;

            supplier.Name = request.Dto.Name;
            supplier.Phone = request.Dto.Phone;
            supplier.Address = request.Dto.Address;

            _unitOfWork.Suppliers.Update(supplier);
            return await _unitOfWork.CompleteAsync() > 0;
        }
    }
}
