using System.Net;
using System.Text.Json;

namespace INVENTO_FLOW.Middleware
{
    // Đây là "Trạm gác" - Global Exception Handler
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        // Constructor
        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next; // _next chính là lệnh "Cho đi tiếp qua trạm"
            _logger = logger; // Dùng để ghi lại chi tiết lỗi ra màn hình Console để IT sửa
        }

        // Bất kỳ đoạn Request (gọi API) nào gửi vào cũng sẽ chạy qua hàm này
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Cho phép Request đi qua để chạy vào Controller và lấy dữ liệu
                // Nếu an toàn không có lỗi, mã sẽ kết thúc tại đây và trả kết quả về user.
                await _next(context);
            }
            catch (Exception ex)
            {
                // BẤT Ỳ CÁI GÌ bị Crash (Code rác, Data null, sập hệ thống...) sẽ được tóm lại ở đây!
                // 1. Ghi lại Log chi tiết đỏ chót ra Console để developer xem
                _logger.LogError(ex, "Lỗi hệ thống bất định: {Message}", ex.Message);
                
                // 2. Chặn lại mọi thông tin nhạy cảm và trả về lỗi chuẩn, thân thiện cho người dùng
                await HandleExceptionAsync(context, ex);
            }
        }

        // Hàm xử lý và biến Lỗi hệ thống thành thông báo JSON dễ đọc
        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // Bắt buộc hệ thống trả dữ liệu ra dạng văn bản chuẩn JSON thay vì mã HTML nhăng nhít
            context.Response.ContentType = "application/json";
            
            // Status code hiển thị 500 - Lỗi máy chủ
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            // Đóng gói thông báo thân thiện
            var errorResponse = new
            {
                StatusCode = context.Response.StatusCode,
                Message = "Hệ thống đang gặp sự cố 1 chút, bạn thử lại sau nhen."
                // Ở dự án thực tế, KHÔNG BAO GIỜ tống 'exception.Message' ra ngoài đây vì sẽ lộ cấu trúc Code.
            };

            // Ép kiểu (Convert) từ Object sang dạng chữ JSON
            var jsonResult = JsonSerializer.Serialize(errorResponse);
            
            // Xịt thẳng chuỗi lỗi này về cho Frontend (như điện thoại, Web, Postman) hiển thị
            return context.Response.WriteAsync(jsonResult);
        }
    }
}
