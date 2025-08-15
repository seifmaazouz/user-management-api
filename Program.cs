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

// Add controller services
builder.Services.AddControllers(); // For API controllers

var app = builder.Build();

// === MIDDLEWARE ===
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<AuthenticationMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

// === ENDPOINTS ===
app.MapGet("/", () => "User Management API - Welcome!");
app.MapGet("/error", () => { throw new Exception("Test exception for global error handler"); });

// Map controllers (uses [Route] attributes)
app.MapControllers();

app.Run();
