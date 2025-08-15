namespace UserManagementAPI.Middleware;

public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _authToken;
    private readonly string[] _publicPaths = { "/", "/error" };

    public AuthenticationMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _authToken = configuration["AuthToken"] ?? "mysecret123";
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip authentication for public endpoints
        if (_publicPaths.Contains(context.Request.Path.Value, StringComparer.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        var token = context.Request.Headers["Authorization"].FirstOrDefault();
        
        if (!IsValidToken(token))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Unauthorized");
            return;
        }

        await _next(context);
    }

    private bool IsValidToken(string? token)
    {
        return !string.IsNullOrEmpty(token) && token == _authToken;
    }
}
