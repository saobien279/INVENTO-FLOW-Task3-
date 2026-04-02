using MediatR;
using InventoFlow.Application.Interfaces.Repositories;
using InventoFlow.Domain.Entities;

namespace InventoFlow.Application.Features.Products.Commands.DeleteProduct
{
    public class DeleteProductHandler : IRequestHandler<DeleteProductCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteProductHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            // 1. Tìm sản phẩm trong kho
            var product = await _unitOfWork.Products.GetByIdAsync(request.Id);
            
            // 2. Nếu không thấy thì báo false (để Controller trả lỗi 404)
            if (product == null) return false;

            // 3. Tiến hành xóa
            _unitOfWork.Products.Delete(product);
            
            // 4. Lưu thay đổi vào DB
            await _unitOfWork.CompleteAsync();
            
            return true;
        }
    }
}
