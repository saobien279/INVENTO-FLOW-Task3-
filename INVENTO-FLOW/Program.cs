using InventoFlow.Infrastructure.Data;
using InventoFlow.Application.Interfaces.Services;
using InventoFlow.Application.Services;
using Microsoft.EntityFrameworkCore;
using InventoFlow.Application.Interfaces.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;
using INVENTO_FLOW.Middleware;
using Serilog;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.ResponseCompression;
using FluentValidation;
using FluentValidation.AspNetCore;
using InventoFlow.Application.Features.Products.Queries.GetProductById;
using InventoFlow.Application;
using InventoFlow.Application.Common.Behaviors;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog for structured logging to console and rolling files
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration)
                 .WriteTo.Console()
                 .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day));

// Register AutoMapper mappings
builder.Services.AddAutoMapper(typeof(InventoFlow.Application.Mappings.MappingProfile));

// Register DbContext with SQL Server connection string
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Application Services and Repositories (Dependency Injection)
builder.Services.AddScoped<IProductRepository, InventoFlow.Infrastructure.Repositories.ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderRepository, InventoFlow.Infrastructure.Repositories.OrderRepository>();
builder.Services.AddScoped<IUserRepository, InventoFlow.Infrastructure.Repositories.UserRepository>();
builder.Services.AddScoped<ICategoryRepository, InventoFlow.Infrastructure.Repositories.CategoryRepository>();
builder.Services.AddScoped<ISupplierRepository, InventoFlow.Infrastructure.Repositories.SupplierRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IUnitOfWork, InventoFlow.Infrastructure.Repositories.UnitOfWork>();

// Configure Validation, MediatR, and Pipeline Behaviors
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetProductByIdQuery).Assembly));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddValidatorsFromAssembly(typeof(ApplicationAssemblyReference).Assembly);

builder.Services.AddControllers();

// Configure CORS Policy for frontend integration (local ReactJS and Vue/Vite clients)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with JWT Bearer authentication scheme
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Enter JWT Bearer token. Example: 'Bearer eyJhbGciOiJIUzI1NiIsIn...' ",
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

// Configure JWT Authentication
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

// Configure Rate Limiting to prevent spam and DDoS
builder.Services.AddRateLimiter(options =>
{
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "application/json; charset=utf-8";
        var responseObj = new { message = "Yêu cầu quá thường xuyên. Vui lòng chậm lại và thử lại sau." };
        await context.HttpContext.Response.WriteAsync(JsonSerializer.Serialize(responseObj), token);
    };

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown_IP",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromSeconds(10),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
});

// Configure Response Compression (Brotli & Gzip)
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

var app = builder.Build();

// Enable Global Exception Handling
app.UseMiddleware<GlobalExceptionMiddleware>();

// Enable Custom Request Logging
app.UseMiddleware<RequestLoggingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseDefaultFiles();
app.UseStaticFiles();

// Apply Response Compression, CORS, Rate Limiting, and Auth Middleware
app.UseResponseCompression();
app.UseCors("AllowSpecificOrigins");
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
