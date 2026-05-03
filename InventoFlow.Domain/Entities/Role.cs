namespace InventoFlow.Domain.Entities
{
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // Ví dụ: "Admin", "User"

        // Quan hệ 1-N: 1 Role có thể được gán cho nhiều User
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
