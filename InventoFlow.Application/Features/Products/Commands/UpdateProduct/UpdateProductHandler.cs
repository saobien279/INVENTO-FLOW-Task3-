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
            var product = await _unitOfWork.Products.GetByIdAsync(request.Id);
            if (product == null) return false;

            // 2. Cập nhật các trường bị thay đổi (chỉ cập nhật nếu có giá trị mới)
            if (!string.IsNullOrWhiteSpace(request.Name)) product.Name = request.Name;
            if (!string.IsNullOrWhiteSpace(request.SKU))  product.SKU = request.SKU;
            product.Price = request.Price;
            product.StockQuantity = request.StockQuantity;

            // 3. Đánh dấu đã thay đổi
            _unitOfWork.Products.Update(product);

            // 4. Lưu xuống DB
            await _unitOfWork.CompleteAsync();
            return true;
        }
    }
}
