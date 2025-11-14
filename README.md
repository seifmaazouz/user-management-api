# User Management API ✅

A simple REST API for managing users built with ASP.NET Core minimal APIs. This project demonstrates middleware implementation, authentication, logging, and CRUD operations.

## Features

- ✅ `CRUD Operations`: Create, Read, Update, Delete users  
- ✅ `Authentication Middleware`: Token-based authentication (configurable)  
- ✅ `Global Exception Handling`: Centralized error handling  
- ✅ `Request/Response Logging`: Comprehensive request/response logging middleware  
- ✅ `Input Validation`: Email format, age limits, username validation  
- ✅ `Thread-Safe Operations`: `ConcurrentDictionary` for data storage  
- ✅ `Comprehensive Testing`: 30+ test cases included (`TestRequests.http`)

## Prerequisites

- .NET 8.0 or later  
- Visual Studio Code with REST Client extension (recommended)

## Quick Start

1. Clone:
```bash
git clone https://github.com/seifmaazouz/user-management-api.git
cd user-management-api
```

2. Run:
```bash
dotnet run
```

3. Test:
- Open `TestRequests.http` in VS Code and use the REST Client "Send Request" buttons.  
- Default base URL: `http://localhost:5070`

## API Endpoints

### Public Endpoints (No Authentication)
- `GET /` — Root endpoint  
- `GET /error` — Trigger global exception handler (testing)

### Protected Endpoints (Requires Authentication)
- `GET /users` — Get all users  
- `GET /users/{id}` — Get user by ID  
- `POST /users` — Create new user  
- `PUT /users/{id}` — Update existing user  
- `DELETE /users/{id}` — Delete user

## Authentication

The application reads the expected token from `appsettings.json` key `AuthToken`:

```json
{
  "AuthToken": "secret-appsettings-token-abc123"
}
```

- If `AuthToken` is present, that value is required in the `Authorization` header.  
- If missing, the application falls back to the default token: `mysecret123`.

Send the token as:
```http
Authorization: secret-appsettings-token-abc123
```

Missing or invalid tokens return `401 Unauthorized`.

## Example Requests

```http
# Get all users
GET http://localhost:5070/users
Authorization: secret-appsettings-token-abc123

# Create user
POST http://localhost:5070/users
Authorization: secret-appsettings-token-abc123
Content-Type: application/json

{
  "username": "John Doe",
  "email": "john@example.com",
  "userAge": 28
}
```

## User Model

```json
{
  "username": "string (1-100 characters, required)",
  "email": "string (valid email format, required)",
  "userAge": "integer (0-150, required)"
}
```

## Middleware Pipeline

Order of middleware (registered in `Program.cs`):

1. `Global Exception Handling` — catches and logs unhandled exceptions  
2. `Authentication` — validates tokens for protected endpoints  
3. `Request/Response Logging` — logs request method/path and response status

## Configuration

- `AuthToken` (appsettings.json): e.g. `secret-appsettings-token-abc123` (fallback: `mysecret123`)  
- `Base URL`: `http://localhost:5070`  
- `MAX_AGE`: `150`  
- `MAX_USERNAME_LENGTH`: `100`

## Validation Rules

- `username`: required, 1–100 characters  
- `email`: required, valid email format (validated server-side)  
- `userAge`: integer between `0` and `150` inclusive

Validation failures return `400 Bad Request` with structured JSON error details.

## Project Structure

```
UserManagementAPI/
├── Program.cs                  # Main application with middleware and endpoints
├── Models/
│   └── User.cs                 # User model class
├── Middleware/                 # Authentication, logging, exception handling
├── Interfaces/                 # Contracts (IUserRepository, etc.)
├── Repositories/               # InMemoryUserRepository (thread-safe)
├── TestRequests.http           # Comprehensive test cases
├── README.md                   # This file
└── TEST-DOCUMENTATION.md       # Test coverage documentation
```

## Sample Data

The API starts with three pre-loaded users:

1. **Alice** (alice@example.com, Age: 30)
2. **Bob** (bob@example.com, Age: 25) 
3. **Charlie** (charlie@example.com, Age: 35)

Initial user IDs start from 1, and new users get auto-incremented IDs starting from 4.

## Error Responses

The API returns consistent JSON error responses:

- `400 Bad Request` — validation errors (JSON)  
- `401 Unauthorized` — missing/invalid token  
- `404 Not Found` — resource not found  
- `500 Internal Server Error` — centralized error handling

Examples:
```json
// Validation error
{ "error": "Username is required and cannot be empty" }

// Not found
{ "error": "User with ID 999 not found" }

// Server error
{ "error": "An internal server error occurred", "details": "..." }
```

## Thread Safety

- Storage: `ConcurrentDictionary<int, User>`  
- Atomic ID generation: `Interlocked.Increment()`  
- Repository implementation is thread-safe for concurrent requests

## Development & Extensibility

- Add middleware in `Middleware/` and register in `Program.cs`.  
- Add endpoints in `Program.cs` (minimal APIs) or `Controllers/` (if using controllers).  
- Implement new persistence in `Repositories/` and register concrete implementation for `IUserRepository` in DI.  
- Update validation in centralized validation helpers.

## Testing

- `TestRequests.http` includes 30+ scenarios (auth, validation, CRUD, edge cases).  
- Configure variables at top of `TestRequests.http`:
```
@baseUrl = http://localhost:5070
@validToken = secret-appsettings-token-abc123
@invalidToken = wrongtoken
```
- Use VS Code REST Client or `curl` to run tests.

## Learning Objectives

This project demonstrates:
- ASP.NET Core minimal APIs  
- Middleware pipeline design and ordering  
- Token-based authentication patterns (configurable via `appsettings.json`)  
- Centralized error handling strategies  
- Input validation techniques and consistent error responses  
- Request and response logging best practices  
- RESTful API design and status-code conventions  
- HTTP testing methodologies (REST Client and request collections)  
- Thread-safe programming and concurrent data structures (`ConcurrentDictionary`, `Interlocked`)  
- Decoupling via interfaces for testability and replaceable persistence

## License

Educational / demo code — use for learning and reference.
