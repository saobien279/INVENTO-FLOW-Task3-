using InventoFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InventoFlow.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Khai báo các bảng trong Database
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<User> Users { get; set; }
        
        // --- 3 BẢNG MỚI ---
        public DbSet<Category> Categories { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Role> Roles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Cấu hình SKU là duy nhất (Unique) để không bị trùng mã hàng
            modelBuilder.Entity<Product>()
                .HasIndex(p => p.SKU)
                .IsUnique();

            // 2. Cấu hình quan hệ 1-N giữa Order và OrderItem
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId);

            // 3. Cấu hình quan hệ 1-N giữa Product và OrderItem
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany() // Một sản phẩm không nhất thiết phải biết nó nằm ở những đơn nào (giữ cho Product gọn sạch)
                .HasForeignKey(oi => oi.ProductId);

            // 4. Cấu hình tiền tệ (Tránh lỗi cảnh báo precision)
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.PriceAtPurchase)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasColumnType("decimal(18,2)");
            // 5. Cấu hình đệ quy cho Category (1 Category có nhiều SubCategories)
            modelBuilder.Entity<Category>()
                .HasOne(c => c.ParentCategory)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentId)
                .OnDelete(DeleteBehavior.Restrict); // Tránh xoá Category cha thì lỡ xoá sạch Category con

            // 6. Cấu hình User -> Role (1-N)
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict); // Không cho xoá Role nếu đang có User dùng

            // 7. Cấu hình Product -> Category & Supplier (1-N)
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.SetNull); // Xoá Category thì Product bị Null (không bị xoá theo)

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Supplier)
                .WithMany(s => s.Products)
                .HasForeignKey(p => p.SupplierId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
