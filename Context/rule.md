# RULE.MD — Quy tắc bắt buộc cho Agent khi làm việc với INVENTO-FLOW

## PHẦN 1: KIẾN TRÚC — LUẬT KHÔNG ĐƯỢC PHÁ VỠ

### R1 — Dependency Flow một chiều
```
WebApi → Application → Domain
WebApi → Infrastructure → Application → Domain
```
- **Application KHÔNG ĐƯỢC** import/reference bất kỳ thứ gì từ Infrastructure.
- **Domain KHÔNG ĐƯỢC** import bất kỳ thứ gì từ bất kỳ tầng nào.
- Khi viết code mới, luôn tự hỏi: "File này đang ở tầng nào, nó có được phép biết về thứ kia không?"

### R2 — Namespace phải khớp tầng
| Tầng | Namespace đúng |
|---|---|
| Domain | `InventoFlow.Domain.Entities` |
| Application | `InventoFlow.Application.*` |
| Infrastructure | `InventoFlow.Infrastructure.*` |
| WebApi | `INVENTO_FLOW.*` |

Không bao giờ dùng namespace cũ `INVENTO_FLOW.Models`, `INVENTO_FLOW.Services`, v.v.

### R3 — TargetFramework đồng nhất
Tất cả 4 project đều dùng **`net8.0`**. Không để lẫn `net10.0` hay version khác.

---

## PHẦN 2: UNIT OF WORK — LUẬT THAO TÁC DỮ LIỆU

### R4 — Services inject IUnitOfWork, không inject Repository trực tiếp
```csharp
// ✅ ĐÚNG
public ProductService(IUnitOfWork unitOfWork, IMapper mapper)

// ❌ SAI — vi phạm UoW pattern
public ProductService(IProductRepository productRepo, IMapper mapper)
```

### R5 — Repository KHÔNG có SaveChangesAsync()
- Các interface `IProductRepository`, `IOrderRepository`, `IUserRepository` **không khai báo** `SaveChangesAsync()`.
- Các class implement cũng **không implement** `SaveChangesAsync()`.
- Việc lưu DB chỉ được thực hiện thông qua `await _unitOfWork.CompleteAsync()`.

### R6 — Mọi thao tác ghi kết thúc bằng CompleteAsync()
```csharp
// ✅ ĐÚNG — pattern chuẩn trong Service
await _unitOfWork.Products.AddAsync(product);
return await _unitOfWork.CompleteAsync() > 0;

// ❌ SAI
await _productRepo.AddAsync(product);
await _productRepo.SaveChangesAsync();
```

### R7 — CompleteAsync() trả về int, phải so sánh để ra bool
```csharp
// ✅ ĐÚNG
return await _unitOfWork.CompleteAsync() > 0;

// ❌ SAI — CS0029 compiler error
return await _unitOfWork.CompleteAsync();
```

---

## PHẦN 3: QUERY DỮ LIỆU — EAGER LOADING

### R8 — Mọi query Order phải Include OrderItems và Product
```csharp
// ✅ ĐÚNG — items sẽ có dữ liệu
_context.Orders
    .Include(o => o.OrderItems!)
    .ThenInclude(oi => oi.Product)
    .ToListAsync()

// ❌ SAI — items: [] rỗng
_context.Orders.ToListAsync()
```

**Áp dụng cho**: `GetAllAsync()`, `GetByUserIdAsync()`, `GetOrderWithDetailsAsync()`.

---

## PHẦN 4: THÊM TÍNH NĂNG MỚI — WORKFLOW BẮT BUỘC

### R9 — Thứ tự triển khai tính năng mới
Khi thêm entity/feature mới (ví dụ: `Category`), **phải đi theo thứ tự**:

1. **Domain**: Tạo Entity `Category.cs`
2. **Application/Interfaces/Repositories**: Tạo `ICategoryRepository.cs` (không có `SaveChangesAsync`)
3. **Application/Interfaces/Services**: Tạo `ICategoryService.cs`
4. **Application/DTOs**: Tạo các DTO cần thiết
5. **Application/Services**: Tạo `CategoryService.cs` (inject `IUnitOfWork`)
6. **Application/Mappings**: Thêm mapping vào `MappingProfile.cs`
7. **Infrastructure/Repositories**: Tạo `CategoryRepository.cs` (không có `SaveChangesAsync`)
8. **Infrastructure**: Cập nhật `UnitOfWork.cs` — thêm property `IcategoryRepository Categories`
9. **Infrastructure/Data**: Thêm `DbSet<Category>` vào `AppDbContext.cs`
10. **WebApi/Program.cs**: Đăng ký DI
11. **WebApi/Controllers**: Tạo `CategoriesController.cs`
12. **Migration**: `dotnet ef migrations add AddCategory --project InventoFlow.Infrastructure --startup-project INVENTO-FLOW`

### R10 — DI Registration tại Program.cs
Mọi service/repository/UoW phải được đăng ký tại `Program.cs` (WebApi). Thiếu đăng ký sẽ gây lỗi runtime `InvalidOperationException`.

---

## PHẦN 5: AUTHENTICATION — JWT

### R11 — AuthService dùng IUnitOfWork, không dùng AppDbContext trực tiếp
```csharp
// ✅ ĐÚNG
public AuthService(IUnitOfWork unitOfWork, IConfiguration config)

// ❌ SAI — vi phạm tầng (Application biết Infrastructure)
public AuthService(AppDbContext context, IConfiguration config)
```

### R12 — Token chứa Role, phải Login lại sau khi đổi Role trong DB
Role được "đúc" vào JWT lúc Login. Nếu update Role trong DB mà không Login lại, Token cũ vẫn giữ Role cũ.

---

## PHẦN 6: NAMESPACE & USING

### R13 — Luôn kiểm tra using khi tạo file mới
Mỗi tầng chỉ được `using` namespace của tầng mình và tầng thấp hơn:
- **Application** có thể dùng: `InventoFlow.Domain.*`, `InventoFlow.Application.*`
- **Infrastructure** có thể dùng: `InventoFlow.Domain.*`, `InventoFlow.Application.*`, `InventoFlow.Infrastructure.*`
- **WebApi** có thể dùng: tất cả

### R14 — Không dùng namespace rác từ thời single-project
Các namespace sau đã bị xóa bỏ hoàn toàn, không bao giờ dùng lại:
- `INVENTO_FLOW.Models`
- `INVENTO_FLOW.Data`
- `INVENTO_FLOW.Services`
- `INVENTO_FLOW.Services.Interfaces`
- `INVENTO_FLOW.Repositories`
- `INVENTO_FLOW.Repositories.Interfaces`
- `INVENTO_FLOW.DTOs`
- `INVENTO_FLOW.Mappings`

---

## PHẦN 7: BUILD & CHẠY

### R15 — App phải được dừng trước khi build
Nếu app đang chạy (process `INVENTO-FLOW` active), lệnh `dotnet build` sẽ lỗi file-lock cho dù code đúng hoàn toàn. Luôn dừng app trước khi build.

### R16 — Lệnh migration chuẩn
```powershell
dotnet ef migrations add <TenMigration> --project InventoFlow.Infrastructure --startup-project INVENTO-FLOW
dotnet ef database update --project InventoFlow.Infrastructure --startup-project INVENTO-FLOW
```

### R17 — Tài khoản Admin phải set thủ công
```sql
UPDATE Users SET Role = 'Admin' WHERE Username = 'ten_tai_khoan';
```
Register API tự động tạo `Role = "User"`. Không có endpoint nào tự tạo Admin.

---

## PHẦN 8: CẬP NHẬT TÀI LIỆU — BẮT BUỘC

### R18 — Cập nhật `agents.md` sau MỌI thay đổi code
Đây là quy tắc **không được bỏ qua**. Sau bất kỳ thao tác nào làm thay đổi code (thêm file, sửa interface, thêm endpoint, đổi logic, thêm field...), Agent **phải cập nhật `agents.md`** để phản ánh đúng trạng thái hiện tại của dự án.

**Các mục thường cần cập nhật:**
- Danh sách API Endpoints nếu thêm/sửa route
- Danh sách Interface nếu thêm/sửa phương thức
- Cấu trúc DTO nếu thêm/sửa field
- Danh sách PageQuery nếu thêm class mới
- Cấu trúc tầng nếu thêm file/class mới

**Thứ tự thực hiện:**
1. Viết/sửa code
2. Build thành công (`dotnet build` → 0 Errors)
3. Cập nhật `agents.md`

