# 📋 Phiên làm việc 29/04/2026 — Tổng kết & Việc cần làm

---

## ✅ Đã hoàn thành trong phiên này

### 1. Cài đặt môi trường
- ✅ Cài .NET 8 SDK (`8.0.420`) qua winget
- ✅ Cài `dotnet-ef` tool (`8.0.26`)
- ✅ `dotnet restore` + `dotnet build` → **Build succeeded, 0 Warning, 0 Error**
- ⚠️ **Lưu ý**: Sau khi cài .NET SDK, cần **mở terminal MỚI** hoặc chạy lệnh refresh PATH:
  ```powershell
  $env:PATH = [System.Environment]::GetEnvironmentVariable("PATH", "Machine") + ";" + [System.Environment]::GetEnvironmentVariable("PATH", "User")
  ```

### 2. Chuyển Database sang SQL Server Express
- ✅ Đổi connection string: `(localdb)\mssqllocaldb` → `localhost\SQLEXPRESS`
- ✅ File: `INVENTO-FLOW/appsettings.json`
  ```json
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=InventoFlowDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
  ```
- ✅ Chạy `dotnet ef database update` → Tạo thành công 4 bảng trên SSMS
- ✅ Áp dụng 3 migrations: `InitialCreate`, `AddUsernameToUser`, `AddUsernameToUserV2`

### 3. Sửa lỗi UnitTests
- ✅ Sửa `InventoFlow.UnitTests.csproj`: `net10.0` → `net8.0`

---

## ❌ Chưa hoàn thành — Việc cần làm phiên sau

### Yêu cầu đề bài (6 tiêu chí)

| # | Tiêu chí | Trạng thái | Ghi chú |
|---|---|---|---|
| 1 | Bài toán thực tế | ❌ Chưa làm | Viết mô tả hệ thống quản lý kho hàng |
| 2 | Liệt kê chức năng | ✅ Đã có | Trong `agents.md` |
| 3 | CSDL ≥ 7 bảng + ràng buộc + data mẫu | ❌ Chưa làm | Hiện chỉ có 4 bảng, cần mở rộng lên 17 bảng |
| 4 | Giao diện UI (1 module CRUD + phân quyền) | ❌ Chưa làm | Cần xây frontend Vanilla HTML/CSS/JS |
| 5 | Link source code | ⚠️ Cần push | Push code mới nhất lên GitHub |
| 6 | Link đóng gói/deploy | ❌ Chưa làm | `dotnet publish` hoặc link localhost |

---

## 🗄️ Thiết kế Database 17 bảng (ĐÃ THỐNG NHẤT)

### 4 bảng hiện có (EF Core quản lý — module hoạt động)

| # | Bảng DB | Entity C# | Cột chính |
|---|---|---|---|
| 1 | `NHANVIEN` | `User` | MaNV, HoTen, TenDangNhap, MatKhau, MaVaiTro(FK) |
| 2 | `SANPHAM` | `Product` | MaSP, TenSP, MaSKU, DonGia, SLTon + (cột NULL cho EF không quản lý) |
| 3 | `HOADON` | `Order` | MaHD, NgayTao, TongTien, MaNV(FK), MaKH(FK→NULL) |
| 4 | `CT_HOADON` | `OrderItem` | MaCTHD(PK), MaHD(FK), MaSP(FK), SoLuong, DonGiaLucBan |

### 13 bảng mới (tạo bằng SQL Script — EF không quản lý)

| # | Bảng | Cột chính | FK |
|---|---|---|---|
| 5 | `DANHMUC` | MaDM, TenDM, MaDanhMucCha | Self-ref FK |
| 6 | `NHACUNGCAP` | MaNCC, TenNCC, DiaChi, SDT | — |
| 7 | `VAITRO` | MaVaiTro, TenVaiTro | — |
| 8 | `KHACHHANG` | MaKH, HoTen, SDT | — |
| 9 | `PHIEUNHAP` | MaPN, NgayNhap, MaNV(FK), MaNCC(FK) | → NHANVIEN, NHACUNGCAP |
| 10 | `CT_PHIEUNHAP` | MaCTPN(PK), MaPN(FK), MaSP(FK), SoLuongNhap, GiaNhap | → PHIEUNHAP, SANPHAM |
| 11 | `PHIEUXUATKHO` | MaPXK, NgayXuat, MaNV(FK), LyDo | → NHANVIEN |
| 12 | `CT_PHIEUXUATKHO` | MaCTPXK(PK), MaPXK(FK), MaSP(FK), SoLuongXuat | → PHIEUXUATKHO, SANPHAM |
| 13 | `PHIEUTRAHANG` | MaPT, NgayTra, MaHD_Goc(FK), MaNV(FK), LyDoTra | → HOADON, NHANVIEN |
| 14 | `CT_PHIEUTRAHANG` | MaCTPT(PK), MaPT(FK), MaSP(FK), SoLuongTra | → PHIEUTRAHANG, SANPHAM |
| 15 | `KHUYENMAI` | MaKM, TenChuongTrinh, NgayBatDau, NgayKetThuc | — |
| 16 | `CT_KHUYENMAI` | MaCTKM(PK), MaKM(FK), MaSP(FK), PhanTramGiam, GiaTriGiam | → KHUYENMAI, SANPHAM |
| 17 | `LOG_HETHONG` | MaLog, MaNV(FK), HanhDong, TenBang, MaDoiTuong | → NHANVIEN |

---

## 🔑 Quyết định thiết kế quan trọng (ĐÃ THỐNG NHẤT)

### 1. Dùng Surrogate Key (Id riêng) thay vì Composite Key
- **Quyết định**: Tất cả bảng chi tiết (CT_HOADON, CT_PHIEUNHAP...) dùng **Id riêng** (auto increment) làm PK
- **Không dùng** composite key `(MaHD, MaSP)` vì:
  - EF Core entity `OrderItem` đã có `Id` → xung đột nếu dùng composite
  - Composite key không cho phép 1 SP xuất hiện 2 lần trong cùng 1 đơn
  - Code phức tạp hơn (FindAsync cần 2 params)
- **Ghi chú báo cáo**: "Sử dụng surrogate key thay cho composite key để tối ưu cho ORM framework"

### 2. Mapping EF Core → Tên tiếng Việt
- Dùng `.ToTable("SANPHAM")` và `.HasColumnName("MaSP")` trong `AppDbContext.OnModelCreating()`
- Code C# vẫn viết `product.Name`, nhưng DB lưu cột `TenSP`
- **Mục đích**: Đồng nhất 17 bảng cùng tên tiếng Việt, không bị lộn xộn 2 ngôn ngữ

### 3. Cột thừa trong bảng EF quản lý
- Bảng SANPHAM có cột `HSD`, `MucTonToiThieu`, `MaDM`, `MaNCC` mà EF không quản lý
- **Giải pháp**: Set `NULL` hoặc `DEFAULT` → EF bỏ qua, không lỗi
- Bảng HOADON có cột `MaKH` mà EF không quản lý → set `NULL`
- `MucTonToiThieu DEFAULT 0` (0 = chưa thiết lập ngưỡng cảnh báo)

### 4. Module hoạt động cho UI
- Chỉ cần **1 module**: Quản lý Sản phẩm (Product CRUD) + Hóa đơn (Order)
- **Phân quyền**: Admin → Thêm/Sửa/Xóa | User → Chỉ xem + đặt hàng
- **Giao diện**: Vanilla HTML/CSS/JS đặt trong `wwwroot/`
- Backend API đã hoàn chỉnh, chỉ cần xây frontend gọi API

---

## 📝 Kế hoạch thực hiện phiên sau

### Phase 1: Database (17 bảng)
- [ ] Viết SQL Script tạo 17 bảng + ràng buộc toàn vẹn
- [ ] Sửa `AppDbContext.OnModelCreating()` — thêm `.ToTable()` + `.HasColumnName()` cho 4 entity
- [ ] Xóa migration cũ, tạo migration mới (hoặc tạo DB hoàn toàn bằng SQL Script)
- [ ] Seed data minh họa cho tất cả 17 bảng

### Phase 2: Giao diện Frontend
- [ ] Tạo `wwwroot/index.html` — Trang Login/Register
- [ ] Tạo `wwwroot/dashboard.html` — Trang chính (bảng sản phẩm, CRUD, phân quyền)
- [ ] Tạo `wwwroot/css/style.css` — Giao diện
- [ ] Tạo `wwwroot/js/auth.js` — Xử lý đăng nhập, lưu JWT token
- [ ] Tạo `wwwroot/js/products.js` — CRUD sản phẩm
- [ ] Tạo `wwwroot/js/api.js` — Helper gọi API với Bearer token
- [ ] Thêm `app.UseStaticFiles()` vào `Program.cs`

### Phase 3: Đóng gói & Nộp bài
- [ ] Viết mô tả bài toán thực tế
- [ ] Push code lên GitHub
- [ ] `dotnet publish` hoặc ghi link localhost

---

## 📁 Cấu trúc project hiện tại

```
InventoFlow.sln
├── InventoFlow.Domain/          ← Entities (4 entity, cần mapping tên Việt)
├── InventoFlow.Application/     ← DTOs, Services, MediatR Handlers, Validators
├── InventoFlow.Infrastructure/  ← EF Core DbContext, Repositories, Migrations
├── INVENTO-FLOW/                ← WebApi (Controllers, Middleware, Program.cs)
│   └── wwwroot/                 ← (CẦN TẠO) Frontend HTML/CSS/JS
├── InventoFlow.UnitTests/       ← Unit Tests (xUnit + Moq)
└── Context/                     ← Tài liệu dự án
    ├── currentwork.md           ← File này
    └── rule.md
```

---

## ⚠️ Lưu ý kỹ thuật
- **Connection**: `localhost\SQLEXPRESS` — Windows Authentication
- **Database**: `InventoFlowDb` (đang có 4 bảng cũ tên tiếng Anh → sẽ tạo lại 17 bảng tên tiếng Việt)
- **Terminal**: Luôn refresh PATH trước khi dùng `dotnet` CLI nếu chưa mở terminal mới
- **Backend API**: Đã chạy thành công tại `http://localhost:5105`
