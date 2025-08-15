using System.Text.Json;
using UserManagementAPI.Models;
using UserManagementAPI.Interfaces;
using UserManagementAPI.Repositories;
using UserManagementAPI.Services;

// === CONFIGURATION ===
const string AUTH_TOKEN = "mysecret123";
const string BASE_URL = "http://localhost:5070";

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls(BASE_URL);

// Add services
// Register repository as singleton - for in-memory storage (Database would use scoped)
builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();
// builder.Services.AddScoped<IUserService, DatabaseUserService>();
// Keep as scoped - Can be singleton for in-memory, but scoped better for future DB integration
builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();

// === HELPER METHODS ===
bool IsValidToken(string? token)
{
    return !string.IsNullOrEmpty(token) && token == AUTH_TOKEN;
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
app.MapGet("/users", async (IUserService service) =>
{
    var users = await service.GetAllUsersAsync();
    return Results.Ok(users);
});

// GET: Retrieve user by ID
app.MapGet("/users/{id:int}", async (int id, IUserService service) =>
{
    var user = await service.GetUserByIdAsync(id);
    if (user != null)
    {
        return Results.Ok(user);
    }
    return Results.NotFound(new { error = $"User with ID {id} not found" });
});

// GET: Check if user exists
app.MapGet("/users/{id:int}/exists", async (int id, IUserService service) =>
{
    bool exists = await service.UserExistsAsync(id);
    return Results.Ok(new { exists, userId = id });
});

// POST: Create new user
app.MapPost("/users", async (User? user, IUserService service) =>
{
    var result = await service.CreateUserAsync(user);
    if (result.success)
    {
        User createdUser = result.user!;
        return Results.Created($"/users/{createdUser.Id}", createdUser);
    }
    return Results.BadRequest(new { error = result.errorMessage });
});

// PUT: Update existing user
app.MapPut("/users/{id:int}", async (int id, User? user, IUserService service) =>
{
    var result = await service.UpdateUserAsync(id, user);
    if (result.success)
    {
        return Results.Ok(result.user);
    }
    return Results.BadRequest(new { error = result.errorMessage });
});

// DELETE: Remove user by ID
app.MapDelete("/users/{id:int}", async (int id, IUserService service) =>
{
    var result = await service.DeleteUserAsync(id);
    if (result.success)
    {
        return Results.NoContent();
    }
    return Results.NotFound(new { error = result.errorMessage });
});

app.Run();
