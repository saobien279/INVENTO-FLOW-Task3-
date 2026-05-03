using InventoFlow.Infrastructure.Data;
using InventoFlow.Application.Interfaces.Services;
using InventoFlow.Application.Services;
using Microsoft.EntityFrameworkCore;
using InventoFlow.Application.Interfaces.Repositories;
using InventoFlow.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using INVENTO_FLOW.Middleware; // Thêm thư viện chứa Trạm Gác của mình
using Serilog; // Thư viện ghi Log siêu cấp
using Microsoft.OpenApi.Models; // Thư viện để cấu hình Swagger Security
using Microsoft.AspNetCore.RateLimiting; // Trạm 3: Thư viện Chống Spam gốc của .NET
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.ResponseCompression; // Trạm 6: Thư viện ép nén dữ liệu
using FluentValidation;
using FluentValidation.AspNetCore;
using InventoFlow.Application.Validators;
using InventoFlow.Application.Features.Products.Queries.GetProductById;
using InventoFlow.Application;
using InventoFlow.Application.Common.Behaviors;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

// Cấu hình thay máu bộ Logging mặc định bằng Serilog để lưu ra File txt
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration)
                 .WriteTo.Console() // Vẫn in màu ra màn hình Console
                 .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)); // Tự động đẻ file mới mỗi ngày trong thư mục Logs/


builder.Services.AddAutoMapper(typeof(InventoFlow.Application.Mappings.MappingProfile)); // Dang ky AutoMapper

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();


builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetProductByIdQuery).Assembly));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddValidatorsFromAssembly(typeof(ApplicationAssemblyReference).Assembly);

// Add services to the container.

builder.Services.AddControllers();

// [XÂY DỰNG TRẠM 4] Thiết lập CORS (Cấp Visa cho Web cố định gọi API)
builder.Services.AddCors(options =>
{
    options.AddPolicy("LopBaoVeWeb", policy =>
    {
        // Chỉ cấp Visa đúng 2 tên miền chạy ReactJS và Vue/Vite ở Local này được phép gọi vào máy chủ
        // Thực tế khi up server, bạn sẽ gõ: "https://tenmiencuaban.com"
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
              .AllowAnyHeader()  // Cho phép mang mọi loại giấy tờ (Bao gồm Giấy chứng nhận JWT Token)
              .AllowAnyMethod(); // Cho phép làm đủ 4 thao tác (Lấy hàng, Thêm hàng, Sửa, Xóa)
    });
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Nhập chuỗi JWT Token của bạn vào bên dưới.\n\nVí dụ: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

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

// [XÂY DỰNG TRẠM 3] Thiết lập bộ máy Chống Spam (Rate Limiting)
builder.Services.AddRateLimiter(options =>
{
    // 1. Cài đặt thông báo trả về khi khách bị khóa (Lỗi 429 Too Many Requests)
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        // Chữ được code cứng chuẩn Tiếng Việt (Thêm bộ giải mã UTF-8 để khỏi lỗi font)
        context.HttpContext.Response.ContentType = "text/plain; charset=utf-8";
        await context.HttpContext.Response.WriteAsync("Bạn đang thao tác quá nhanh chớp nhoáng (Spam/DDoS)! Vui lòng chậm lại 10 giây chờ hệ thống thở nhé.", token);
    };

    // 2. Tạo một luật Global (Toàn cục) áp dụng chặt cho mọi IP khách hàng
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            // Lấy địa chỉ IP của mạng người dùng làm chìa khóa nhận diện
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown_IP",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10, // Chỉ cho phép tối đa 10 nhấp chuột (Requests)
                Window = TimeSpan.FromSeconds(10), // Trong khoảng thời gian đếm ngược 10 giây
                QueueLimit = 0, // Hàng chờ số lượng 0 (Hễ ấn lần thứ 11 là đá văng thẳng cổ, không cho xếp hàng chờ)
                AutoReplenishment = true
            }));
});

// [XÂY DỰNG TRẠM 6] Thiết lập Máy nén dữ liệu API (Response Compression)
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true; // Cho phép máy nén chạy cả trên kết nối an toàn HTTPS
    options.Providers.Add<BrotliCompressionProvider>(); // Setup thuật toán ép nén hiện đại mạnh nhất Brotli
    options.Providers.Add<GzipCompressionProvider>();   // Setup Gzip dể phòng hờ lỡ máy khách không có Brotli
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

// 6. Cắm Trạm Gác Số 6 - Máy Ép Nén Response Compression
// Đặt nó ở cửa ngõ đi ra để mọi dữ liệu kết quả xuất ra (JSON) đều bị ép mỏng lại
app.UseResponseCompression();

// 4. Cắm Trạm Gác Số 4 (CORS) - "Xét hỏi giấy thông hành của Trình duyệt Web"
// Phải đứng từ xa chặn lại ngay nếu thấy trang Web lạ gọi API
app.UseCors("LopBaoVeWeb");

// 3. Cắm Trạm Gác Số 3 (Chống Spam) xuống dây chuyền (Pipeline)
// Kéo trạm gác này đóng chốt ở đây để chặn đứng trước khi cho khách bưng Database đi
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
