using MediatR;
using InventoFlow.Application.Interfaces.Repositories;
using InventoFlow.Domain.Entities;

namespace InventoFlow.Application.Features.Auth.Commands.Register
{
    public class RegisterHandler : IRequestHandler<RegisterCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        public RegisterHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            // 1. Kiểm tra Username đã tồn tại chưa
            if (await _unitOfWork.Users.AnyUsernameAsync(request.Username))
                return false;

            // 2. Tạo User mới và Hash mật khẩu bằng BCrypt
            var user = new User
            {
                Username = request.Username,
                RoleId = 1, // Mặc định ID 1 là Role User
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
            };

            // 3. Lưu xuống Database
            await _unitOfWork.Users.AddAsync(user);
            return await _unitOfWork.CompleteAsync() > 0;
        }
    }
}
