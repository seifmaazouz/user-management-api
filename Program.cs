using System.Collections.Concurrent;
using System.Text.Json;

// Configuration constants
const string AUTH_TOKEN = "mysecret123";
const string BASE_URL = "http://localhost:5070";
const int MAX_AGE = 150;
const int MAX_USERNAME_LENGTH = 100;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls(BASE_URL);

var app = builder.Build();

// Global exception handling middleware
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        // Log the error
        app.Logger.LogError(ex, "Error occurred for {Method} {Path}",
            context.Request.Method,
            context.Request.Path);

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
    // Skip auth for public endpoints
    if (context.Request.Path == "/" || context.Request.Path == "/error")
    {
        await next();
        return;
    }

    var token = context.Request.Headers["Authorization"].FirstOrDefault();
    
    if (!isValidToken(token))
    {
        context.Response.StatusCode = 401;
        await context.Response.WriteAsync("Unauthorized");
        return;
    }

    await next();
});

// Request/Response logging middleware
app.Use(async (context, next) =>
{
    var logger = app.Logger;
    
    logger.LogInformation("Request Method: {Method}, Path: {Path}", context.Request.Method, context.Request.Path);
    
    await next();
    
    logger.LogInformation("Response Status: {StatusCode}", context.Response.StatusCode);
});

// Thread-safe dictionary for concurrent access - NOW WITH IDs IN USER OBJECTS
var users = new ConcurrentDictionary<int, User>
{
    [1] = new User { Id = 1, Username = "Alice", Email = "alice@example.com", UserAge = 30 },
    [2] = new User { Id = 2, Username = "Bob", Email = "bob@example.com", UserAge = 25 },
    [3] = new User { Id = 3, Username = "Charlie", Email = "charlie@example.com", UserAge = 35 }
};

// Thread-safe auto-incrementing ID counter
int nextUserId = users.Count + 1;

// Thread-safe ID generation
int GetNextUserId()
{
    return Interlocked.Increment(ref nextUserId) - 1;
}

bool isValidToken(string? token)
{
    return !string.IsNullOrEmpty(token) && token == AUTH_TOKEN;
}

// Validate user data
    (bool isValid, string errorMessage) ValidateUser(User? user)
{
    if (user == null)
        return (false, "User data is required");
    
    if (string.IsNullOrWhiteSpace(user.Username))
        return (false, "Username is required and cannot be empty");
    
    if (user.Username.Length > MAX_USERNAME_LENGTH)
        return (false, "Username cannot exceed 100 characters");
    
    if (string.IsNullOrWhiteSpace(user.Email))
        return (false, "Email is required and cannot be empty");
    
    if (!IsValidEmail(user.Email))
        return (false, "Email format is invalid");
    
    if (user.UserAge < 0)
        return (false, "Age cannot be negative");
    
    if (user.UserAge > MAX_AGE)
        return (false, "Age cannot exceed 150");
    
    return (true, string.Empty);
}

// Validate email format
bool IsValidEmail(string email)
{
    try
    {
        var addr = new System.Net.Mail.MailAddress(email);
        return addr.Address == email;
    }
    catch
    {
        return false;
    }
}

app.MapGet("/", () => "This is the root endpoint!");

// Test endpoint for global exception handler
app.MapGet("/error", () => { throw new Exception("This is a test exception to trigger the global error handler."); });

// GET: Retrieve all users
app.MapGet("/users", () =>
{
    return Results.Ok(users.Values.ToList());
});

// GET: Retrieve user by ID
app.MapGet("/users/{id:int}", (int id) =>
{
    if (users.TryGetValue(id, out var user))
    {
        return Results.Ok(user);
    }
    return Results.NotFound(new { error = $"User with ID {id} not found" });
});

// POST: Create a new user - NOW SETS THE ID ON THE USER OBJECT
app.MapPost("/users", (User? user) =>
{
    var validation = ValidateUser(user);
    if (!validation.isValid)
    {
        return Results.BadRequest(new { error = validation.errorMessage });
    }
    
    int newId = GetNextUserId();
    user!.Id = newId; // Set the ID on the user object
    users.TryAdd(newId, user);
    return Results.Created($"/users/{newId}", user); // Return the user with ID
});

// PUT: Update an existing user - NOW PRESERVES THE ID
app.MapPut("/users/{id:int}", (int id, User? user) =>
{
    if (!users.ContainsKey(id))
    {
        return Results.NotFound(new { error = $"User with ID {id} not found" });
    }
    
    var validation = ValidateUser(user);
    if (!validation.isValid)
    {
        return Results.BadRequest(new { error = validation.errorMessage });
    }
    
    user!.Id = id; // Ensure the user object has the correct ID
    users.TryUpdate(id, user, users[id]);
    return Results.Ok(user); // Return the updated user with ID
});

// DELETE: Remove a user by ID
app.MapDelete("/users/{id:int}", (int id) =>
{
    if (users.TryRemove(id, out var removedUser))
    {
        return Results.NoContent();
    }
    return Results.NotFound(new { error = $"User with ID {id} not found" });
});

app.Run();
