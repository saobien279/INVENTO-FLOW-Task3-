public class OrderQueryParams
{
    // Lọc theo UserId (Admin xem đơn của 1 user cụ thể, null = lấy tất cả)
    public int? UserId { get; set; }

    // Sắp xếp (VD: "date_asc", "date_desc", "amount_asc", "amount_desc")
    public string? SortBy { get; set; }

    // Phân trang
    public int PageNumber { get; set; } = 1;  // Mặc định trang 1
    public int PageSize   { get; set; } = 10; // Mặc định 10 đơn/trang
}
