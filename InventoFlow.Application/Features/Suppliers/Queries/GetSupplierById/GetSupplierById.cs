using AutoMapper;
using InventoFlow.Application.DTOs.Supplier;
using InventoFlow.Application.Interfaces.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace InventoFlow.Application.Features.Suppliers.Queries.GetSupplierById
{
    public record GetSupplierByIdQuery(int Id) : IRequest<SupplierResponseDto?>;

    public class GetSupplierByIdHandler : IRequestHandler<GetSupplierByIdQuery, SupplierResponseDto?>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetSupplierByIdHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<SupplierResponseDto?> Handle(GetSupplierByIdQuery request, CancellationToken cancellationToken)
        {
            var supplier = await _unitOfWork.Suppliers.GetByIdAsync(request.Id);
            if (supplier == null) return null;

            return _mapper.Map<SupplierResponseDto>(supplier);
        }
    }
}
