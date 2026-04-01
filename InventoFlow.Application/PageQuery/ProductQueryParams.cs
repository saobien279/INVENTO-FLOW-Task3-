public class ProductQueryParams
{
    // Tìm kiếm
    public string? SearchTerm { get; set; }

    // Sắp xếp (VD: "price_asc", "price_desc", "name")
    public string? SortBy { get; set; }

    // Phân trang
    public int PageNumber { get; set; } = 1; // Mặc định trang 1
    public int PageSize { get; set; } = 10;  // Mặc định lấy 10 món
}