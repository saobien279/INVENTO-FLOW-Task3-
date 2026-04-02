using MediatR;
using AutoMapper;
using InventoFlow.Application.Interfaces.Repositories;
using InventoFlow.Application.DTOs.Product;

namespace InventoFlow.Application.Features.Products.Queries.GetProductById
{
public class GetProductByIdHandler : IRequestHandler<GetProductByIdQuery, ProductResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetProductByIdHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ProductResponseDto> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(request.Id);
        if (product == null) throw new Exception("Không tìm thấy sản phẩm");

        return _mapper.Map<ProductResponseDto>(product);
    }
}
}