using InventoFlow.Application.Interfaces.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace InventoFlow.Application.Features.Categories.Commands.UpdateCategory
{
    public class UpdateCategoryHandler : IRequestHandler<UpdateCategoryCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateCategoryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(request.Dto.Id);
            if (category == null) return false;

            category.Name = request.Dto.Name;
            category.ParentId = request.Dto.ParentId;

            _unitOfWork.Categories.Update(category);
            return await _unitOfWork.CompleteAsync() > 0;
        }
    }
}
