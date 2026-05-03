namespace InventoFlow.Domain.Entities
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // Đệ quy: trỏ về Category cha
        public int? ParentId { get; set; }
        public Category? ParentCategory { get; set; }

        // Danh sách danh mục con
        public ICollection<Category> SubCategories { get; set; } = new List<Category>();
        
        // Danh sách sản phẩm thuộc danh mục này
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
