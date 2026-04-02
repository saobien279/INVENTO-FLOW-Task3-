user: admin
password : 12345

---
# PROJECT CONTEXT SUMMARY (INVENTO-FLOW)
*File này là "bộ nhớ" của dự án. Agent đọc file này trước khi làm bất kỳ thao tác nào.*

---

## 1. Kiến trúc tổng quan — 4-Layer Clean Architecture

```
InventoFlow.sln
├── InventoFlow.Domain           (Tầng 1 - Domain)
├── InventoFlow.Application      (Tầng 2 - Application / Business Logic)
├── InventoFlow.Infrastructure   (Tầng 3 - Infrastructure / EF Core)
└── INVENTO-FLOW                 (Tầng 4 - WebApi / Presentation)
```

### Luồng phụ thuộc (một chiều)
```
WebApi → Application → Domain
WebApi → Infrastructure → Application → Domain
```
> ⚠️ Application KHÔNG được tham chiếu tới Infrastructure. Domain không tham chiếu ai.

### TargetFramework
Tất cả 4 project đều dùng **`net8.0`**.

---

## 2. Cấu trúc file chi tiết từng tầng

### Tầng 1 — `InventoFlow.Domain`
**Namespace**: `InventoFlow.Domain.Entities` | **Packages**: Không có

#### Entities
| Class | Các property chính |
|---|---|
| `User` | `Id`, `Username`, `Name`, `PasswordHash`, `Role`, `Orders?` |
| `Product` | `Id`, `Name`, `SKU`, `Price`, `StockQuantity` |
| `Order` | `Id`, `OrderDate`, `TotalAmount`, `UserId`, `OrderItems?` |
| `OrderItem` | `Id`, `OrderId`, `Order?`, `ProductId`, `Product?`, `Quantity`, `PriceAtPurchase` |

---

### Tầng 2 — `InventoFlow.Application`
**Namespace**: `InventoFlow.Application.*` | **ProjectReference**: `InventoFlow.Domain`

**Packages**: `AutoMapper.Extensions.Microsoft.DependencyInjection`, `BCrypt.Net-Next`, `FluentValidation.AspNetCore`, `System.IdentityModel.Tokens.Jwt`, `Microsoft.Extensions.Configuration.Abstractions`

#### DTOs (`InventoFlow.Application.DTOs.*`)
| File | Namespace | Các field |
|---|---|---|
| `UserLoginDto` | `DTOs.User` | `Username`, `Password` |
| `UserRegisterDto` | `DTOs.User` | `Username`, `Password` |
| `ProductCreateDto` | `DTOs.Product` | `Name`, `SKU`, `Price`, `StockQuantity` |
| `ProductUpdateDto` | `DTOs.Product` | `Id`, `Name?`, `SKU?`, `Price`, `StockQuantity`, computed `StockStatus` |
| `ProductResponseDto` | `DTOs.Product` | `Id`, `Name`, `SKU`, `Price`, `StockQuantity` |
| `OrderCreateDto` | `DTOs.Order` | `UserId`, `Items: List<OrderItemCreateDto>` |
| `OrderItemCreateDto` | `DTOs.Order` | `ProductId`, `Quantity` |
| `OrderResponseDto` | `DTOs.Order` | `Id`, `OrderDate`, `TotalAmount`, `UserId`, `Items: List<OrderItemResponseDto>` |
| `OrderItemResponseDto` | `DTOs.Order` | `ProductId`, `ProductName?`, `Quantity`, `PriceAtPurchase`, computed `SubTotal` |

#### Interfaces/Repositories (`InventoFlow.Application.Interfaces.Repositories`)
```csharp
// IProductRepository — KHÔNG có SaveChangesAsync()
GetAllAsync(ProductQueryParams query) → (IEnumerable<Product> Items, int TotalCount)
GetByIdAsync(int id), GetBySKUAsync(string sku), AddAsync, Update, Delete

// IOrderRepository — KHÔNG có SaveChangesAsync()
GetAllAsync(OrderQueryParams query) → (IEnumerable<Order> Items, int TotalCount)
AddAsync, GetByIdAsync, GetByUserIdAsync, GetOrderWithDetailsAsync

// IUserRepository — KHÔNG có SaveChangesAsync()
AnyUsernameAsync, GetByUsernameAsync, AddAsync

// IUnitOfWork
Products, Orders, Users, CompleteAsync(), BeginTransactionAsync(),
CommitTransactionAsync(), RollbackTransactionAsync()
```

#### Interfaces/Services (`InventoFlow.Application.Interfaces.Services`)
```csharp
IProductService
  CreateProductAsync(ProductCreateDto)
  GetAllProductsAsync(ProductQueryParams)  → PagedResult<ProductResponseDto>
  GetProductByIdAsync(int id)
  UpdateProductAsync(int id, ProductUpdateDto)
  DeleteProductAsync(int id)

IOrderService
  CreateOrderAsync(OrderCreateDto)
  GetAllOrdersAsync(OrderQueryParams)      → PagedResult<OrderResponseDto>
  GetOrderByIdAsync(int id)
  GetOrdersByUserIdAsync(int userId)

IAuthService
  RegisterAsync(UserRegisterDto)
  LoginAsync(UserLoginDto)
```

#### Services (`InventoFlow.Application.Services`)
- Tất cả inject **`IUnitOfWork`** (không inject Repository riêng lẻ)
- `AuthService` inject `IUnitOfWork` + `IConfiguration`
- `ProductService`, `OrderService` inject `IUnitOfWork` + `IMapper`
- Mọi thao tác ghi kết thúc bằng `await _unitOfWork.CompleteAsync() > 0`

#### PageQuery (`InventoFlow.Application.PageQuery`)
| Class | Các field |
|---|---|
| `PagedResult<T>` | `Items`, `TotalCount`, `PageNumber`, `PageSize`, computed `TotalPages` |
| `ProductQueryParams` | `SearchTerm?`, `SortBy?` (name_desc/price_asc/price_desc), `PageNumber=1`, `PageSize=10` |
| `OrderQueryParams` | `UserId?`, `SortBy?` (date_asc/date_desc/amount_asc/amount_desc), `PageNumber=1`, `PageSize=10` |

#### Mappings (`InventoFlow.Application.Mappings`)
```csharp
// MappingProfile.cs
CreateMap<ProductCreateDto, Product>();
CreateMap<ProductUpdateDto, Product>();
CreateMap<Product, ProductResponseDto>();
CreateMap<Order, OrderResponseDto>()
    .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.OrderItems));
CreateMap<OrderItem, OrderItemResponseDto>()
    .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product!.Name));
```
> `ForMember` cần thiết vì `Order.OrderItems` ≠ `OrderResponseDto.Items` và `OrderItem.Product.Name` cần "phẳng hóa" xuống `ProductName`.

#### Validators (`InventoFlow.Application.Validators`)
| Validator | Validate cho | Các rule |
|---|---|---|
| `ProductCreateValidator` | `ProductCreateDto` | Name NotEmpty/MaxLength(100), Price > 0, StockQuantity >= 0 |
| `OrderCreateValidator` | `OrderCreateDto` | UserId > 0, Items NotEmpty, mỗi item: ProductId > 0 & Quantity > 0 |

---

### Tầng 3 — `InventoFlow.Infrastructure`
**Namespace**: `InventoFlow.Infrastructure.*` | **ProjectReference**: `InventoFlow.Application`

**Packages**: `Microsoft.EntityFrameworkCore.SqlServer 8.0.0`, `Tools 8.0.0`, `Design 8.0.0`

#### `AppDbContext` (`InventoFlow.Infrastructure.Data`)
- `DbSet<Product>`, `DbSet<Order>`, `DbSet<OrderItem>`, `DbSet<User>`
- Fluent API: SKU unique, quan hệ 1-N Order↔OrderItem, 1-N Product↔OrderItem
- `decimal(18,2)` cho `Price`, `PriceAtPurchase`, `TotalAmount`

#### Repositories (`InventoFlow.Infrastructure.Repositories`)
- `ProductRepository`, `OrderRepository`, `UserRepository` — **KHÔNG có `SaveChangesAsync()`**
- `ProductRepository.GetAllAsync(query)` hỗ trợ: Search (Name/SKU), Sort, Paging → trả Tuple
- `OrderRepository.GetAllAsync(query)` hỗ trợ: Filter UserId, Sort, Paging → trả Tuple, **có đầy đủ `.Include(OrderItems).ThenInclude(Product)`**
- `OrderRepository`: `GetByUserIdAsync()`, `GetOrderWithDetailsAsync()` đều có `.Include(o => o.OrderItems!).ThenInclude(oi => oi.Product)` → đảm bảo `items` không rỗng

#### `UnitOfWork` (`InventoFlow.Infrastructure.Repositories`)
```csharp
public class UnitOfWork : IUnitOfWork
{
    public IProductRepository Products { get; private set; }
    public IOrderRepository Orders { get; private set; }
    public IUserRepository Users { get; private set; }

    public UnitOfWork(AppDbContext context) {
        // Khởi tạo 3 repo, truyền chung 1 context → đảm bảo 1 transaction
        Products = new ProductRepository(context);
        Orders   = new OrderRepository(context);
        Users    = new UserRepository(context);
    }

    public async Task<int> CompleteAsync() => await _context.SaveChangesAsync();
    // + BeginTransactionAsync, CommitTransactionAsync, RollbackTransactionAsync
}
```

---

### Tầng 4 — `INVENTO-FLOW` (WebApi)
**Namespace**: `INVENTO_FLOW.*` | **ProjectReferences**: `InventoFlow.Application`, `InventoFlow.Infrastructure`

**Packages**: `Microsoft.AspNetCore.Authentication.JwtBearer 8.0.0`, `Swashbuckle.AspNetCore 6.6.2`, `Serilog.AspNetCore 10.0.0`, `Microsoft.EntityFrameworkCore.Design 8.0.0`

#### Controllers
| Controller | Route gốc | [Authorize] | Các action |
|---|---|---|---|
| `AuthController` | `api/auth` | Có (class), AllowAnonymous cho từng action | `Register` (POST), `Login` (POST) |
| `ProductsController` | `api/products` | Có (class) | `GetAll`, `GetById`, `Create`(Admin), `UpdateProduct`(Admin), `Delete`(Admin) |
| `OrdersController` | `api/orders` | Có (class) | `GetAll`(Admin), `Create`, `GetById`, `GetByUserId` |

#### Middleware
- `GlobalExceptionMiddleware` — bắt exception, trả JSON `{ StatusCode, Message }` với HTTP 500
- `RequestLoggingMiddleware` — đo thời gian, log 🟢 thành công / 🔴 thất bại kèm ms

---

## 3. DI Registration (Program.cs — thứ tự đăng ký)

```csharp
builder.Services.AddAutoMapper(typeof(InventoFlow.Application.Mappings.MappingProfile));
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories riêng lẻ (để linh hoạt nếu cần inject trực tiếp)
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// UnitOfWork (pattern chính)
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IOrderService, OrderService>();
```

---

## 4. Middleware Pipeline (thứ tự quan trọng)

```
1. GlobalExceptionMiddleware     ← BẮT BUỘC đầu tiên
2. RequestLoggingMiddleware
3. Swagger (Dev only)
4. UseHttpsRedirection
5. UseResponseCompression        ← Brotli + Gzip
6. UseCors("LopBaoVeWeb")        ← localhost:3000 & localhost:5173
7. UseRateLimiter                ← 10 req/10s per IP → 429 nếu vượt
8. UseAuthentication
9. UseAuthorization
10. MapControllers
```

---

## 5. Authentication & JWT

```
Login → AuthService.LoginAsync()
      → BCrypt.Verify(password, hash)
      → CreateToken(user)
         ├── Claims: NameIdentifier, Name, Role
         ├── SigningKey: appsettings.json["Jwt:Key"]
         ├── Issuer: appsettings.json["Jwt:Issuer"]
         ├── Audience: appsettings.json["Jwt:Audience"]
         └── Expires: 1 ngày
      → Trả về chuỗi JWT

Gọi API sau đó:
      Header: Authorization: Bearer <TOKEN>
      → UseAuthentication() kiểm tra chữ ký bằng cùng Jwt:Key
      → [Authorize(Roles = "Admin")] kiểm tra Claim Role
```

> ⚠️ Role đóng gói vào token lúc Login. Đổi Role trong DB phải **Login lại** để có token mới.

---

## 6. Database

- **DB**: SQL Server — connection string `DefaultConnection` trong `appsettings.json`
- **Migration**: `dotnet ef migrations add <TenMigration> --project InventoFlow.Infrastructure --startup-project INVENTO-FLOW`
- **Update DB**: `dotnet ef database update --project InventoFlow.Infrastructure --startup-project INVENTO-FLOW`

---

## 7. API Endpoints đầy đủ

| Method | Route | Auth | Query Params | Mô tả |
|---|---|---|---|---|
| POST | `/api/auth/register` | Anonymous | — | Đăng ký (Role = "User") |
| POST | `/api/auth/login` | Anonymous | — | Đăng nhập → nhận JWT |
| GET | `/api/products` | User/Admin | `searchTerm`, `sortBy`, `pageNumber`, `pageSize` | Danh sách SP có phân trang |
| GET | `/api/products/{id}` | User/Admin | — | Chi tiết 1 sản phẩm |
| POST | `/api/products` | **Admin** | — | Thêm sản phẩm |
| PUT | `/api/products/{id}` | **Admin** | — | Cập nhật (id route phải khớp dto.Id) |
| DELETE | `/api/products/{id}` | **Admin** | — | Xóa sản phẩm |
| GET | `/api/orders` | **Admin** | `userId`, `sortBy`, `pageNumber`, `pageSize` | Tất cả đơn hàng có phân trang |
| POST | `/api/orders` | User/Admin | — | Tạo đơn hàng (trừ tồn kho tự động) |
| GET | `/api/orders/{id}` | User/Admin | — | Chi tiết đơn hàng (kèm items) |
| GET | `/api/orders/user/{userId}` | User/Admin | — | Đơn hàng theo UserId (kèm items) |

---

## 8. Tài khoản Admin

Register tự gán `Role = "User"`. Để có Admin phải chạy SQL thủ công:
```sql
UPDATE Users SET Role = 'Admin' WHERE Username = 'ten_tai_khoan';
```

---

## 9. Serilog & Logging

- Ghi log ra Console và file `Logs/log-{date}.txt` (rolling theo ngày)
- Cấu hình trong `Program.cs` qua `builder.Host.UseSerilog(...)`

---

## 10. FluentValidation

- Đăng ký qua `builder.Services.AddFluentValidationAutoValidation()` (tự động validate DTO khi Controller nhận request)
- Validator nằm ở tầng **Application** (`InventoFlow.Application.Validators`)
- Khi validation fail → API tự trả về `400 Bad Request` với danh sách lỗi

---

## 11. MediatR / CQRS Pattern (Kiến trúc Command/Query)

### Tổng quan
Toàn bộ Controller đã được **refactor sang MediatR**. Mỗi Controller chỉ inject **`IMediator`** duy nhất và gọi `_mediator.Send(command/query)`. Logic nghiệp vụ nằm hoàn toàn trong các Handler.

### Nguyên tắc đặt tên
- **Command**: Hành động thay đổi dữ liệu (Create, Update, Delete)
- **Query**: Hành động đọc dữ liệu (GetAll, GetById, GetByUserId)
- **Handler**: Lớp xử lý tương ứng với mỗi Command/Query

### Cấu trúc thư mục Features
```
InventoFlow.Application/Features/
├── Auth/
│   └── Commands/
│       ├── Login/
│       │   ├── LoginCommand.cs         → IRequest<string?>
│       │   └── LoginHandler.cs
│       └── Register/
│           ├── RegisterCommand.cs      → IRequest<bool>
│           └── RegisterHandler.cs
├── Products/
│   ├── Commands/
│   │   ├── CreateProduct/
│   │   │   ├── CreateProductCommand.cs → IRequest<ProductResponseDto>
│   │   │   └── CreateProductHandler.cs
│   │   ├── UpdateProduct/
│   │   │   ├── UpdateProductCommand.cs → IRequest<bool>
│   │   │   └── UpdateProductHandler.cs
│   │   └── DeleteProduct/
│   │       ├── DeleteProductCommand.cs → IRequest<bool>
│   │       └── DeleteProductHandler.cs
│   └── Queries/
│       ├── GetAllProducts/
│       │   ├── GetAllProductsQuery.cs  → IRequest<PagedResult<ProductResponseDto>>
│       │   └── GetAllProductsHandler.cs
│       └── GetProductById/
│           ├── GetProductByIdQuery.cs  → IRequest<ProductResponseDto>
│           └── GetProductByIdHandler.cs
└── Orders/
    ├── Commands/
    │   └── CreateOrder/
    │       ├── CreateOrderCommand.cs   → IRequest<OrderResponseDto>
    │       └── CreateOrderHandler.cs
    └── Queries/
        ├── GetAllOrders/
        │   ├── GetAllOrdersQuery.cs    → IRequest<PagedResult<OrderResponseDto>>
        │   └── GetAllOrdersHandler.cs
        ├── GetOrderById/
        │   ├── GetOrderByIdQuery.cs    → IRequest<OrderResponseDto?>
        │   └── GetOrderByIdHandler.cs
        └── GetOrdersByUserId/
            ├── GetOrdersByUserIdQuery.cs → IRequest<IEnumerable<OrderResponseDto>>
            └── GetOrdersByUserIdHandler.cs
```

### Đăng ký MediatR trong Program.cs
```csharp
// Quét toàn bộ Assembly của Application để đăng ký tất cả Handler tự động
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(GetProductByIdQuery).Assembly));
```
> ⚠️ Chỉ cần 1 dòng này, MediatR tự tìm và đăng ký tất cả Handler trong toàn bộ Assembly.

### Controllers sau refactor
| Controller | Inject | Thay đổi |
|---|---|---|
| `AuthController` | `IMediator` | Không còn `IAuthService` |
| `ProductsController` | `IMediator` | Không còn `IProductService` |
| `OrdersController` | `IMediator` | Không còn `IOrderService` |

> ✅ `IProductService`, `IOrderService` vẫn được đăng ký trong DI (để `AuthHandler` tái sử dụng), nhưng Controller không còn phụ thuộc trực tiếp vào chúng.

### Unit Test với Features/Handlers
Unit test có thể test từng Handler riêng lẻ mà không cần Controller — đây là lợi thế lớn nhất của CQRS.