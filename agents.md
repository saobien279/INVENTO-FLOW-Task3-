user: admin
password : 12345

---
# PROJECT CONTEXT SUMMARY (INVENTO-FLOW)
*Tóm tắt dự án để các session sau có thể hiểu kiến trúc và flow hiện tại.*

## 1. Kiến trúc tổng quan
Dự án được xây dựng theo mô hình **ASP.NET Core Web API** (sử dụng .NET 8), kết hợp với **Entity Framework Core** trên cơ sở dữ liệu **SQL Server**.
Cấu trúc dự án tuân thủ mô hình layered architecture (Controller -> Service -> Repository / Database):
- **Controllers**: Nơi tiếp nhận các Request API (`AuthController`, `OrdersController`, `ProductsController`).
- **Services**: Chứa Business Logic, đứng giữa Controller và Repository (`AuthService`, `OrderService`, `ProductService`).
- **Repositories**: Chịu trách nhiệm trực tiếp gọi Entity Framework để lấy/ghi dữ liệu từ Database (`OrderRepository`, `ProductRepository`).
- **Models / Entities**: Định nghĩa các bảng Database: `User`, `Product`, `Order`, `OrderItem`.
- **DTOs**: Dùng để truyền nhận dữ liệu API thay vì dùng trực tiếp Model/Entity (ví dụ: `UserLoginDto`, `OrderCreateDto`, `ProductResponseDto`...).

## 2. Authentication & Security
- Ứng dụng sử dụng **JWT (JSON Web Token)** để bảo mật. Key và Issuer được lưu trong `appsettings.json`.
- Mật khẩu của người dùng được mã hóa bằng **BCrypt** trước khi lưu vào Database (`AuthService`).
- **Quan trọng**: Toàn bộ các controller (`ProductsController`, `OrdersController`, `AuthController`) hiện tại đều đã được bảo vệ bằng attribute `[Authorize]`.
- Riêng 2 endpoint đăng nhập và đăng ký trong `AuthController` được gắn `[AllowAnonymous]` để có thể gọi mà không cần token.
- Phải dùng token (Bearer Token) trên Auth Header để gọi các API thuộc Products và Orders.

## 3. Database Context
- Cấu hình Database nằm trong class `AppDbContext` (EF Core).
- Các mối quan hệ đã được map chuẩn xác qua Fluent API (`OnModelCreating`):
  - SKU của `Product` là duy nhất (`IsUnique()`).
  - `Order` 1-N `OrderItem`.
  - `Product` 1-N `OrderItem`.

## 4. Ghi chú cho kế hoạch ở session sau
- Để test các API (Products, Orders), **phải** tạo 1 Request Login thông qua file POST `api/auth/login` bằng tài khoản đã có (hoặc User đăng ký qua `api/auth/register`), lấy chuỗi Token, gắn vô header `Authorization: Bearer <TOKEN>` cho các Request tiếp theo.
- AutoMapper cũng đã được sử dụng trong codebase để map từ Model sang DTO.

## 5. Middleware Pipeline
- Đã thiết lập **GlobalExceptionMiddleware** (Trạm bắt lỗi toàn cục): Tự động tóm gọn mọi exception do Backend văng ra, ghi Log qua `ILogger` và trả về một Error Response chuẩn dạng JSON (HTTP 500) an toàn thay vì văng HTML hiển thị stack code. Trạm này đã được cấu hình cắm chốt ngay đầu Pipeline trong `Program.cs`.
- Đã thiết lập **RequestLoggingMiddleware** (Trạm lưu vết hệ thống): Tự động bấm giờ đo thời gian phản hồi (ms) của mọi Request và in log lưu vết cùng HTTP Status Code ra Console (xanh lá nếu chạy mượt, vàng/đỏ nếu có lỗi), giúp truy vết lịch sử gọi API cực nhanh.
- Tích hợp thành công **Serilog** để tự động xuất và chuẩn hóa toàn bộ Log thống kê của hệ thống (bao gồm 2 trạm Middleware trên) ra file vật lý lưu trữ vĩnh viễn theo chu kỳ tự động tách file theo từng ngày trong folder `/Logs/log-yyyyMMdd.txt`.