using AutoMapper;
using InventoFlow.Application.DTOs.Supplier;
using InventoFlow.Application.Interfaces.Repositories;
using InventoFlow.Domain.Entities;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace InventoFlow.Application.Features.Suppliers.Commands.CreateSupplier
{
    public record CreateSupplierCommand(SupplierCreateDto Dto) : IRequest<SupplierResponseDto>;

    public class CreateSupplierHandler : IRequestHandler<CreateSupplierCommand, SupplierResponseDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CreateSupplierHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<SupplierResponseDto> Handle(CreateSupplierCommand request, CancellationToken cancellationToken)
        {
            var supplier = _mapper.Map<Supplier>(request.Dto);
            await _unitOfWork.Suppliers.AddAsync(supplier);
            await _unitOfWork.CompleteAsync();
            return _mapper.Map<SupplierResponseDto>(supplier);
        }
    }
}
