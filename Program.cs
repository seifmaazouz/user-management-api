using UserManagementAPI.Models;
using UserManagementAPI.Interfaces;
using UserManagementAPI.Repositories;
using UserManagementAPI.Services;
using UserManagementAPI.Middleware;

// === CONFIGURATION ===
const string BASE_URL = "http://localhost:5070";
// Configuration automatically reads from appsettings.json for settings like AuthToken (not good for production, but fine for learning)
// for production, use environment variables or secure vaults

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls(BASE_URL);

// Add services
// Register repository as singleton - for in-memory storage (Database would use scoped)
builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();
// Keep as scoped - Can be singleton for in-memory, but scoped better for future DB integration
builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();

// === MIDDLEWARE ===
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<AuthenticationMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

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
