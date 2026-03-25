using InventoFlow.Application.DTOs.User;
using InventoFlow.Application.Interfaces.Repositories;
using InventoFlow.Application.Interfaces.Services;
using InventoFlow.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace InventoFlow.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepo;
        private readonly IConfiguration _config;

        public AuthService(IUserRepository userRepo, IConfiguration config)
        {
            _userRepo = userRepo;
            _config = config;
        }

        public async Task<bool> RegisterAsync(UserRegisterDto dto)
        {
            //Khong trung username
            if (await _userRepo.AnyUsernameAsync(dto.Username))
                return false;

            var user = new User
            {
                Username = dto.Username,
                Role = "User",
                // Thực hiện Hash mật khẩu bằng thư viện BCrypt
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            await _userRepo.AddAsync(user);
            return await _userRepo.SaveChangesAsync() > 0;
        }

        public async Task<string?> LoginAsync(UserLoginDto dto)
        {
            var user = await _userRepo.GetByUsernameAsync(dto.Username);

            // Kiểm tra User tồn tại và Verify mật khẩu đã hash
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return null;

            return CreateToken(user);
        }

        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };

            // Lấy Secret Key (chữ ký admin) từ appsettings.json
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1), // Token hết hạn sau 1 ngày
                SigningCredentials = creds,
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}