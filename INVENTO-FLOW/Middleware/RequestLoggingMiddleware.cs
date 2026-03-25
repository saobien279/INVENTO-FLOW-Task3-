using System.Diagnostics;

namespace INVENTO_FLOW.Middleware
{
    // Đây là "Trạm gác số 2" - Nhận diện, lưu vết dấu chân khách (Request Logging & Tracking)
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger; // Cuốn sổ ghi chép nhật ký hành trình hệ thống
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // 1. CHUẨN BỊ GHI HÌNH TRƯỚC KHI VÀO TRONG: Bấm đồng hồ tính giờ
            var stopwatch = Stopwatch.StartNew();

            // Nhặt thông tin cơ bản xem ông khách này làm gì, gọi cửa nào
            var method = context.Request.Method; // GET, POST, PUT, DELETE...
            var path = context.Request.Path;     // Ví dụ: /api/products

            try
            {
                // 2. CHO PHÉP KHÁCH VÀO BÊN TRONG (Đi qua các Trạm gác khác và vào Controller)
                await _next(context);
            }
            finally
            {
                // 3. LÚC KHÁCH BƯỚC RA VÀ LẤY ĐƯỢC KẾT QUẢ VỀ (Bất kể thành công hay sập lỗi)
                // Dừng đồng hồ lại để xem mất bao lâu để xử lý
                stopwatch.Stop();
                var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

                // Xem thử thẻ kết quả mã trả về cho khách là nhiêu (200 OK, 400 Lỗi, 500 Sập)
                var statusCode = context.Response.StatusCode;

                // 4. GHI NHẬT KÝ CHI TIẾT
                // Log ra báo cáo: 
                // "Khách gọi hàm POST vô đường dẫn /api/orders hoàn tất với mã 201 trong 45ms"
                if (statusCode >= 400)
                {
                    // Lỗi 400, 401, 403, 500 thì in màu Vàng Cảnh Bảo hoặc Đỏ để dể nhìn
                    _logger.LogWarning(
                        "🔴 [THẤT BẠI] HTTP {Method} {Path} -> Phản hồi {StatusCode} (Thời gian xử lý: {ElapsedMilliseconds}ms)",
                        method, path, statusCode, elapsedMilliseconds);
                }
                else
                {
                    // Trạng thái mượt 200, 201 thì in màu Trắng/Xanh bình thường
                    _logger.LogInformation(
                        "🟢 [THÀNH CÔNG] HTTP {Method} {Path} -> Phản hồi {StatusCode} (Thời gian xử lý: {ElapsedMilliseconds}ms)",
                        method, path, statusCode, elapsedMilliseconds);
                }
            }
        }
    }
}
