using System.Text.Json;
using System.Collections.Concurrent;
using UserManagementAPI.Models;
using System.Net.Mail;

// === CONFIGURATION ===
const string AUTH_TOKEN = "mysecret123";
const string BASE_URL = "http://localhost:5070";
const int MAX_AGE = 150;
const int MAX_USERNAME_LENGTH = 100;
const int MIN_PASSWORD_LENGTH = 6;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls(BASE_URL);

var app = builder.Build();

// === DATA STORE ===
// Thread-safe in-memory data store with sample users
var users = new ConcurrentDictionary<int, User>
{
    [1] = new User { Id = 1, Username = "Alice", Email = "alice@example.com", Age = 30, Password = "password123" },
    [2] = new User { Id = 2, Username = "Bob", Email = "bob@example.com", Age = 25, Password = "password123" },
    [3] = new User { Id = 3, Username = "Charlie", Email = "charlie@example.com", Age = 35, Password = "password123" }
};

// Thread-safe auto-incrementing ID counter starting after existing users
int nextUserId = users.Count + 1;

// === HELPER METHODS ===
int GetNextUserId()
{
    return Interlocked.Increment(ref nextUserId);
}

bool IsValidToken(string? token)
{
    return !string.IsNullOrEmpty(token) && token == AUTH_TOKEN;
}

bool IsValidEmail(string email)
{
    try
    {
        var addr = new MailAddress(email);
        return addr.Address == email;
    }
    catch
    {
        return false;
    }
}

(bool isValid, string errorMessage) ValidateUser(User? user)
{
    if (user == null)
        return (false, "User data is required");
    
    if (string.IsNullOrWhiteSpace(user.Username))
        return (false, "Username is required and cannot be empty");
    
    if (user.Username.Length > MAX_USERNAME_LENGTH)
        return (false, $"Username cannot exceed {MAX_USERNAME_LENGTH} characters");
    
    if (string.IsNullOrWhiteSpace(user.Email))
        return (false, "Email is required and cannot be empty");
    
    if (!IsValidEmail(user.Email))
        return (false, "Email format is invalid");
    
    if (user.Age < 0 || user.Age > MAX_AGE)
        return (false, $"Age must be between 0 and {MAX_AGE}");
    
    if (string.IsNullOrWhiteSpace(user.Password))
        return (false, "Password is required and cannot be empty");
    
    if (user.Password.Length < MIN_PASSWORD_LENGTH)
        return (false, $"Password must be at least {MIN_PASSWORD_LENGTH} characters long");
    
    return (true, string.Empty);
}

// === MIDDLEWARE ===
// Global exception handling
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Error occurred for {Method} {Path}",
            context.Request.Method, context.Request.Path);

        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";

        var response = new
        {
            error = "An internal server error occurred",
            details = app.Environment.IsDevelopment() ? ex.Message : "Please try again later"
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
});

// Authentication middleware
app.Use(async (context, next) =>
{
    // Skip authentication for public endpoints
    if (context.Request.Path == "/" || context.Request.Path == "/error")
    {
        await next();
        return;
    }

    var token = context.Request.Headers["Authorization"].FirstOrDefault();
    
    if (!IsValidToken(token))
    {
        context.Response.StatusCode = 401;
        await context.Response.WriteAsync("Unauthorized");
        return;
    }

    await next();
});

// Request/Response logging
app.Use(async (context, next) =>
{
    var logger = app.Logger;
    
    logger.LogInformation("Request: {Method} {Path}", context.Request.Method, context.Request.Path);
    
    await next();
    
    logger.LogInformation("Response: {StatusCode}", context.Response.StatusCode);
});

// === ENDPOINTS ===
app.MapGet("/", () => "User Management API - Welcome!");

app.MapGet("/error", () => { throw new Exception("Test exception for global error handler"); });

// GET: Retrieve all users
app.MapGet("/users", () => Results.Ok(users.Values));

// GET: Retrieve user by ID
app.MapGet("/users/{id:int}", (int id) =>
{
    if (users.TryGetValue(id, out var user))
    {
        return Results.Ok(user);
    }
    return Results.NotFound(new { error = $"User with ID {id} not found" });
});

// POST: Create new user
app.MapPost("/users", (User? user) =>
{
    var validation = ValidateUser(user);
    if (!validation.isValid)
    {
        return Results.BadRequest(new { error = validation.errorMessage });
    }
    
    int newId = GetNextUserId();
    var newUser = user! with { Id = newId };
    users.TryAdd(newId, newUser); // Remove unnecessary check - ID is always unique
    return Results.Created($"/users/{newId}", newUser);
});

// PUT: Update existing user
app.MapPut("/users/{id:int}", (int id, User? user) =>
{
    // Check if user exists first
    if (!users.ContainsKey(id))
    {
        return Results.NotFound(new { error = $"User with ID {id} not found" });
    }
    
    var validation = ValidateUser(user);
    if (!validation.isValid)
    {
        return Results.BadRequest(new { error = validation.errorMessage });
    }
    
    var userToStore = user! with { Id = id };
    users.TryUpdate(id, userToStore, users[id]);
    return Results.Ok(userToStore);
});

// DELETE: Remove user by ID
app.MapDelete("/users/{id:int}", (int id) =>
{
    if (users.TryRemove(id, out _))
    {
        return Results.NoContent();
    }
    return Results.NotFound(new { error = $"User with ID {id} not found" });
});

app.Run();
