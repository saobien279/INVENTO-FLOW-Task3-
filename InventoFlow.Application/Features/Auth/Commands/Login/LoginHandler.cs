using MediatR;
using InventoFlow.Application.Interfaces.Repositories;
using InventoFlow.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace InventoFlow.Application.Features.Auth.Commands.Login
{
    public class LoginHandler : IRequestHandler<LoginCommand, string?>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _config;

        public LoginHandler(IUnitOfWork unitOfWork, IConfiguration config)
        {
            _unitOfWork = unitOfWork;
            _config = config;
        }

        public async Task<string?> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            // 1. Tìm User theo Username
            var user = await _unitOfWork.Users.GetByUsernameAsync(request.Username);

            // 2. Kiểm tra mật khẩu bằng BCrypt
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return null;

            // 3. Tạo và trả về JWT token
            return CreateToken(user);
        }

        // Hàm tạo JWT token (chuyển thẳng từ AuthService vào đây)
        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role?.Name ?? "User")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(1),
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
