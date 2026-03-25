using INVENTO_FLOW.DTOs.User;

namespace INVENTO_FLOW.Services.Interfaces
{
    public interface IAuthService
    {
        Task<bool> RegisterAsync(UserRegisterDto dto);
        Task<string?> LoginAsync(UserLoginDto dto); // Trả về chuỗi Token nếu thành công
    }
}
