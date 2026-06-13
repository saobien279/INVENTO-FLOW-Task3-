using System.Diagnostics;

namespace INVENTO_FLOW.Middleware
{
    // Middleware for logging incoming HTTP requests and measuring execution time
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var method = context.Request.Method;
            var path = context.Request.Path;

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();
                var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
                var statusCode = context.Response.StatusCode;

                // Log the outcome based on HTTP status code
                if (statusCode >= 400)
                {
                    _logger.LogWarning(
                        "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMilliseconds} ms",
                        method, path, statusCode, elapsedMilliseconds);
                }
                else
                {
                    _logger.LogInformation(
                        "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMilliseconds} ms",
                        method, path, statusCode, elapsedMilliseconds);
                }
            }
        }
    }
}
