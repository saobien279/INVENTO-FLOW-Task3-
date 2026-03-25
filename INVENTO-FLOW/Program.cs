using INVENTO_FLOW.Data;
using INVENTO_FLOW.Services.Interfaces;
using INVENTO_FLOW.Services;
using Microsoft.EntityFrameworkCore;
using INVENTO_FLOW.Repositories.Interfaces;
using INVENTO_FLOW.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using INVENTO_FLOW.Middleware; // Thêm thư viện chứa Trạm Gác của mình
using Serilog; // Thư viện ghi Log siêu cấp

var builder = WebApplication.CreateBuilder(args);

// Cấu hình thay máu bộ Logging mặc định bằng Serilog để lưu ra File txt
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration)
                 .WriteTo.Console() // Vẫn in màu ra màn hình Console
                 .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)); // Tự động đẻ file mới mỗi ngày trong thư mục Logs/


builder.Services.AddAutoMapper(typeof(INVENTO_FLOW.Mappings.MappingProfile)); // Dang ky AutoMapper
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IOrderService, OrderService>();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

var app = builder.Build();

// 1. Cắm Trạm Gác (Middleware) Bắt lỗi toàn cục vào ĐẦU TIÊN của đường ống (Pipeline)
// Bất cứ ai gọi API cũng phải đi qua để trạm gác này "Bảo kê"
app.UseMiddleware<GlobalExceptionMiddleware>();

// 2. Cắm Trạm Gác Số 2: Ghi hình, Theo dõi nhật ký (Logging) đo thời gian xử lý Server
app.UseMiddleware<RequestLoggingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
