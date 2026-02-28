using System.Net;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;

namespace DIFC.API.Middleware
{
    /// <summary>
    /// Catches any unhandled exception thrown anywhere in the app
    /// and returns a clean JSON error response instead of crashing
    /// or leaking stack traces to the client.
    /// 
    /// WHY middleware instead of try/catch everywhere?
    /// Middleware wraps the ENTIRE request pipeline. One piece of code
    /// handles all errors globally. Without this, unhandled exceptions
    /// in .NET return an ugly HTML error page (or expose stack traces
    /// in development mode in production — a security risk).
    /// 
    /// HOW middleware works:
    /// Request → [This Middleware] → [Auth] → [Routing] → [Controller]
    /// Response ←                 ←        ←            ←
    /// 
    /// Each middleware calls _next(context) to pass control to the next
    /// middleware. By wrapping _next in try/catch, we intercept any
    /// exception that bubbles up from anything downstream.
    /// </summary>
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
                await _next(context); // Run the rest of the pipeline
            }
            catch (Exception ex)
            {
                // Log the full exception with stack trace for debugging
                _logger.LogError(ex, "Unhandled exception on {Method} {Path}", context.Request.Method, context.Request.Path);

                await HandleExceptionAsync(context);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var response = new
            {
                statusCode = 500,
                message = "An unexpected error occurred. Please try again later."
                // NEVER expose ex.Message in production — it can leak
                // internal implementation details (table names, file paths etc.)
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
