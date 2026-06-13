using System.Net;
using System.Text.Json;

namespace INVENTO_FLOW.Middleware
{
    // Global Exception Handling Middleware
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                // Log detailed error information internally
                _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
                
                // Return a clean, user-friendly JSON error response
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var errorResponse = new
            {
                StatusCode = context.Response.StatusCode,
                Message = "Đã xảy ra lỗi hệ thống. Vui lòng liên hệ quản trị viên hoặc thử lại sau."
            };

            var jsonResult = JsonSerializer.Serialize(errorResponse);
            return context.Response.WriteAsync(jsonResult);
        }
    }
}
