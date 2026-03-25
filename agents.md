user: admin
password : 12345

---
# PROJECT CONTEXT SUMMARY (INVENTO-FLOW)
*Tóm tắt dự án để các session sau có thể hiểu kiến trúc và flow hiện tại.*

## 1. Kiến trúc tổng quan — 4-Layer Clean Architecture

Dự án đã được **tái cấu trúc thành 4 tầng** theo mô hình **Clean Architecture** với solution gồm 4 project riêng biệt:

```
InventoFlow.sln
├── InventoFlow.Domain           (Tầng 1 - Domain)
├── InventoFlow.Application      (Tầng 2 - Application)
├── InventoFlow.Infrastructure   (Tầng 3 - Infrastructure)
└── INVENTO-FLOW                 (Tầng 4 - WebApi / Presentation)
```

### Luồng phụ thuộc (Dependency Flow)
```
WebApi → Application → Domain
WebApi → Infrastructure → Application → Domain
```
> Infrastructure biết về Application để triển khai interface. Application KHÔNG biết về Infrastructure. Domain hoàn toàn độc lập.

---

## 2. Chi tiết từng tầng

### Tầng 1 — `InventoFlow.Domain` (net8.0)
- **Mục đích**: Chứa các Entity thuần túy, không phụ thuộc thư viện ngoài.
- **Namespace**: `InventoFlow.Domain.Entities`
- **Files**: `User.cs`, `Product.cs`, `Order.cs`, `OrderItem.cs`

### Tầng 2 — `InventoFlow.Application` (net8.0)
- **Mục đích**: Chứa logic nghiệp vụ (Services), Interfaces, DTOs và Mapping.
- **Cấu trúc chính**:
  - `Interfaces/Repositories/` — Định nghĩa các "hợp đồng" lưu trữ (e.g., `IUserRepository`).
  - `Services/` — Triển khai logic, sử dụng Repository Interface (e.g., `AuthService`).
  - `Mappings/` — Cấu hình AutoMapper (`MappingProfile`).
- **Packages**: `AutoMapper`, `BCrypt.Net-Next`, `FluentValidation`, `JwtSystem`.
- **ProjectReference**: `InventoFlow.Domain`

### Tầng 3 — `InventoFlow.Infrastructure` (net8.0)
- **Mục đích**: Triển khai các interface từ Application. Chứa EF Core, DbContext, Repositories thực tế.
- **Namespace**: `InventoFlow.Infrastructure.*`
- **Packages**: `Microsoft.EntityFrameworkCore.SqlServer`, `Tools`, `Design`.
- **ProjectReferences**: `InventoFlow.Application`

### Tầng 4 — `INVENTO-FLOW` (WebApi - net8.0)
- **Mục đích**: Controllers, Middlewares, Cấu hình DI và Pipeline.
- **DI Registration**: Mọi service và repository phải được đăng ký tại đây (`Program.cs`).
- **ProjectReferences**: `InventoFlow.Application`, `InventoFlow.Infrastructure`

---

## 3. Authentication & Security (JWT Flow)
- **Tạo Token**: `AuthService` dùng `IConfiguration` để đọc Secret Key từ `appsettings.json`, tạo JWT chứa Role (Admin/User).
- **Xác thực**: Token được gửi qua Header `Authorization: Bearer <TOKEN>`.
- **Phân quyền**: Sử dụng `[Authorize(Roles = "Admin")]` tại Controller để chặn các truy cập không hợp lệ.

---

## 4. Database & EF Core
- **Database**: SQL Server.
- `AppDbContext` tại `InventoFlow.Infrastructure.Data`.
- **Lệnh Migration**: `dotnet ef migrations add <Name> --project InventoFlow.Infrastructure --startup-project INVENTO-FLOW`

---

## 5. Middleware Pipeline
1. `GlobalExceptionMiddleware` — Bắt lỗi toàn cục.
2. `RequestLoggingMiddleware` — Ghi log thời gian phản hồi.
3. `ResponseCompression` — Nén dữ liệu đầu ra.
4. `CORS` (Policy: `LopBaoVeWeb`).
5. `RateLimiter` — Chống Spam (10 req/10s).
6. `Authentication` & `Authorization`.

---

## 6. Lưu ý cho session sau
- Hệ thống đã sạch sẽ, các folder `Services` và `Repositories` rỗng ở tầng WebApi đã được xóa.
- Khi thêm tính năng mới, luôn bắt đầu bằng cách định nghĩa Interface trong tầng `Application` trước khi triển khai trong `Infrastructure`.
- TK Admin mặc định cần được set thủ công trong DB (loại Role = "Admin").