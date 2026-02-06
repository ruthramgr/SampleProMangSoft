using System.Text.Json;

namespace APIPMSoftware.Src.Infrastructure.ExternalService
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly IWebHostEnvironment _env;

        public GlobalExceptionMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionMiddleware> logger,
            IWebHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred");
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;

                // DEV environment? show detailed error
                if (_env.IsDevelopment())
                {
                    var devResponse = new
                    {
                        error = ex.Message,
                        stackTrace = ex.StackTrace,
                        inner = ex.InnerException?.Message
                    };

                    await context.Response.WriteAsync(
                        JsonSerializer.Serialize(devResponse));
                }
                else
                {
                    // PROD environment: generic message
                    var prodResponse = new
                    {
                        error = "An unexpected error occurred. Please try again later."
                    };

                    await context.Response.WriteAsync(
                        JsonSerializer.Serialize(prodResponse));
                }
            }
        }
    }

}
