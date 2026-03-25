using InventoFlow.Application.DTOs.User;

namespace InventoFlow.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<bool> RegisterAsync(UserRegisterDto dto);
        Task<string?> LoginAsync(UserLoginDto dto);
    }
}