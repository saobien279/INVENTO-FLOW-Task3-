namespace InventoFlow.Application.DTOs.Category
{
    public class CategoryCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public int? ParentId { get; set; }
    }
}
