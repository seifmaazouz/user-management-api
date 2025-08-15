using System.Text.Json;

namespace UserManagementAPI.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
            _logger.LogError(ex, "Error occurred for {Method} {Path}",
                context.Request.Method, context.Request.Path);

            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";

            var response = new
            {
                error = "An internal server error occurred",
                details = context.RequestServices.GetRequiredService<IWebHostEnvironment>()
                    .IsDevelopment() ? ex.Message : "Please try again later"
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
