using InventoFlow.Application.Interfaces.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace InventoFlow.Application.Features.Suppliers.Commands.DeleteSupplier
{
    public record DeleteSupplierCommand(int Id) : IRequest<bool>;

    public class DeleteSupplierHandler : IRequestHandler<DeleteSupplierCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteSupplierHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(DeleteSupplierCommand request, CancellationToken cancellationToken)
        {
            var supplier = await _unitOfWork.Suppliers.GetByIdAsync(request.Id);
            if (supplier == null) return false;

            _unitOfWork.Suppliers.Delete(supplier);
            return await _unitOfWork.CompleteAsync() > 0;
        }
    }
}
