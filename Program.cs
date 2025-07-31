using System.Collections.Concurrent;
using System.Text.Json;

// Configuration
const string AUTH_TOKEN = "mysecret123";
const int MAX_AGE = 150;
const int MAX_USERNAME_LENGTH = 100;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Middleware
app.UseGlobalExceptionHandling();
app.UseSimpleAuthentication();
app.UseRequestResponseLogging();

// Data storage
var users = new ConcurrentDictionary<int, User>
{
    [1] = new User { Username = "Alice", Email = "alice@example.com", UserAge = 30 },
    [2] = new User { Username = "Bob", Email = "bob@example.com", UserAge = 25 },
    [3] = new User { Username = "Charlie", Email = "charlie@example.com", UserAge = 35 }
};

int nextUserId = 4;
int GetNextUserId() => Interlocked.Increment(ref nextUserId);

// Helper methods
NotFound<object> UserNotFound(int id) => Results.NotFound(new { error = $"User with ID {id} not found" });
BadRequest<object> ValidationError(string message) => Results.BadRequest(new { error = message });

// Validation
(bool isValid, string errorMessage) ValidateUser(User? user)
{
    if (user == null) return (false, "User data is required");
    if (string.IsNullOrWhiteSpace(user.Username)) return (false, "Username is required");
    if (user.Username.Length > MAX_USERNAME_LENGTH) return (false, "Username too long");
    if (string.IsNullOrWhiteSpace(user.Email)) return (false, "Email is required");
    if (!IsValidEmail(user.Email)) return (false, "Invalid email format");
    if (user.UserAge < 0 || user.UserAge > MAX_AGE) return (false, "Invalid age");
    return (true, string.Empty);
}

bool IsValidEmail(string email)
{
    try { return new System.Net.Mail.MailAddress(email).Address == email; }
    catch { return false; }
}

// Endpoints
app.MapGet("/", () => "User Management API");
app.MapGet("/error", () => throw new Exception("Test exception"));

app.MapGet("/users", () => Results.Ok(users.Values.ToList()));

app.MapGet("/users/{id:int}", (int id) =>
    users.TryGetValue(id, out var user) ? Results.Ok(user) : UserNotFound(id));

app.MapPost("/users", (User? user) =>
{
    var validation = ValidateUser(user);
    if (!validation.isValid) return ValidationError(validation.errorMessage);
    
    int newId = GetNextUserId();
    users.TryAdd(newId, user!);
    return Results.Created($"/users/{newId}", user);
});

app.MapPut("/users/{id:int}", (int id, User? user) =>
{
    if (!users.ContainsKey(id)) return UserNotFound(id);
    
    var validation = ValidateUser(user);
    if (!validation.isValid) return ValidationError(validation.errorMessage);
    
    users.TryUpdate(id, user!, users[id]);
    return Results.Ok(user);
});

app.MapDelete("/users/{id:int}", (int id) =>
    users.TryRemove(id, out _) ? Results.NoContent() : UserNotFound(id));

app.Run();

// Middleware extensions
static class MiddlewareExtensions
{
    public static void UseGlobalExceptionHandling(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            try
            {
                await next();
            }
            catch (Exception ex)
            {
                app.Logger.LogError(ex, "Error: {Method} {Path}", context.Request.Method, context.Request.Path);
                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";
                
                var response = new { error = "Internal server error" };
                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
        });
    }

    public static void UseSimpleAuthentication(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            if (context.Request.Path == "/" || context.Request.Path == "/error")
            {
                await next();
                return;
            }

            var token = context.Request.Headers["Authorization"].FirstOrDefault();
            if (token != AUTH_TOKEN)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized");
                return;
            }

            await next();
        });
    }

    public static void UseRequestResponseLogging(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            app.Logger.LogInformation("{Method} {Path}", context.Request.Method, context.Request.Path);
            await next();
            app.Logger.LogInformation("Response: {StatusCode}", context.Response.StatusCode);
        });
    }
}
