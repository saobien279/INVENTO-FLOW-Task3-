using MediatR;
using InventoFlow.Application.Interfaces.Repositories;
using InventoFlow.Domain.Entities;

namespace InventoFlow.Application.Features.Products.Commands.UpdateProduct
{
    public class UpdateProductHandler : IRequestHandler<UpdateProductCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateProductHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            // 1. Tìm sản phẩm cần sửa
            var product = await _unitOfWork.Products.GetByIdAsync(request.Dto.Id);
            if (product == null) return false;

            // 2. Cập nhật các trường bị thay đổi
            if (!string.IsNullOrWhiteSpace(request.Dto.Name)) product.Name = request.Dto.Name;
            if (!string.IsNullOrWhiteSpace(request.Dto.SKU))  product.SKU = request.Dto.SKU;
            product.Price = request.Dto.Price;
            product.StockQuantity = request.Dto.StockQuantity;
            product.CategoryId = request.Dto.CategoryId;
            product.SupplierId = request.Dto.SupplierId;

            // 3. Đánh dấu đã thay đổi
            _unitOfWork.Products.Update(product);

            // 4. Lưu xuống DB
            await _unitOfWork.CompleteAsync();
            return true;
        }
    }
}
