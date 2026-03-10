namespace INVENTO_FLOW.Models
{
    public class User
    {
        public int Id { get; set; } // Viết hoa Id để EF tự hiểu là Primary Key
        public string Name { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;

        // Nên thêm quan hệ ngược lại với Order để dễ truy vấn
        public ICollection<Order>? Orders { get; set; }
    }
}
