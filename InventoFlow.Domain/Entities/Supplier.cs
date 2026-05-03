namespace InventoFlow.Domain.Entities
{
    public class Supplier
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Address { get; set; }

        // Danh sách sản phẩm của nhà cung cấp này
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
