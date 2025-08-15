using System.Text.Json;
using UserManagementAPI.Models;
using UserManagementAPI.Interfaces;
using UserManagementAPI.Repositories;
using System.Net.Mail;

// === CONFIGURATION ===
const string AUTH_TOKEN = "mysecret123";
const string BASE_URL = "http://localhost:5070";
const int MAX_AGE = 150;
const int MAX_USERNAME_LENGTH = 100;
const int MIN_PASSWORD_LENGTH = 6;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls(BASE_URL);

// Register repository as singleton - for in-memory storage (Database would use scoped)
builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();

var app = builder.Build();

// === HELPER METHODS ===
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
app.MapGet("/users", async (IUserRepository repository) =>
{
    var users = await repository.GetAllUsersAsync();
    return Results.Ok(users);
});

// GET: Retrieve user by ID
app.MapGet("/users/{id:int}", async (int id, IUserRepository repository) =>
{
    var user = await repository.GetUserByIdAsync(id);
    if (user != null)
    {
        return Results.Ok(user);
    }
    return Results.NotFound(new { error = $"User with ID {id} not found" });
});

// POST: Create new user
app.MapPost("/users", async (User? user, IUserRepository repository) =>
{
    var validation = ValidateUser(user);
    if (!validation.isValid)
    {
        return Results.BadRequest(new { error = validation.errorMessage });
    }
    
    var newUser = await repository.CreateUserAsync(user!);
    return Results.Created($"/users/{newUser.Id}", newUser);
});

// PUT: Update existing user
app.MapPut("/users/{id:int}", async (int id, User? user, IUserRepository repository) =>
{
    if (!await repository.UserExistsAsync(id))
    {
        return Results.NotFound(new { error = $"User with ID {id} not found" });
    }
    
    var validation = ValidateUser(user);
    if (!validation.isValid)
    {
        return Results.BadRequest(new { error = validation.errorMessage });
    }
    
    var updatedUser = await repository.UpdateUserAsync(id, user!);
    return Results.Ok(updatedUser);
});

// DELETE: Remove user by ID
app.MapDelete("/users/{id:int}", async (int id, IUserRepository repository) =>
{
    if (await repository.DeleteUserAsync(id))
    {
        return Results.NoContent();
    }
    return Results.NotFound(new { error = $"User with ID {id} not found" });
});

app.Run();
