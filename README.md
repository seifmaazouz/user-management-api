# User Management API ✅

A simple REST API for managing users built with ASP.NET Core minimal APIs. This project demonstrates middleware implementation, authentication, logging, and CRUD operations.

## Features

- ✅ **CRUD Operations**: Create, Read, Update, Delete users  
- ✅ **Authentication Middleware**: Simple token-based authentication  
- ✅ **Global Exception Handling**: Centralized error handling  
- ✅ **Request/Response Logging**: Comprehensive logging middleware  
- ✅ **Input Validation**: Email format, age limits, username validation  
- ✅ **Thread-Safe Operations**: Concurrent dictionary for data storage  
- ✅ **Comprehensive Testing**: 30+ test cases included

## Prerequisites

- .NET 8.0 or later  
- Visual Studio Code with REST Client extension (for testing)

## Quick Start

1. **Clone the repository**
   ```bash
   git clone https://github.com/seifmaazouz/user-management-api.git
   cd user-management-api
   ```

2. **Run the application**
   ```bash
   dotnet run
   ```

3. **Test the API**
   - Open `TestRequests.http` in VS Code
   - Click "Send Request" above any test case
   - API runs on `http://localhost:5070`

## API Endpoints

### Public Endpoints (No Authentication)
- `GET /` - Root endpoint  
- `GET /error` - Test exception handling

### Protected Endpoints (Requires Authentication)
- `GET /users` - Get all users  
- `GET /users/{id}` - Get user by ID  
- `POST /users` - Create new user  
- `PUT /users/{id}` - Update existing user  
- `DELETE /users/{id}` - Delete user

## Authentication

The application reads the expected token from `appsettings.json` using the key:

```json
{
  "AuthToken": "secret-appsettings-token-abc123"
}
```

- If `AuthToken` is present in `appsettings.json`, that value is used as the required token.
- If `AuthToken` is not configured, the application falls back to the default token: `mysecret123`.

Send the token in the `Authorization` header:

```http
Authorization: secret-appsettings-token-abc123
```

Requests with a missing or invalid token will return `401 Unauthorized`.

## Example Requests

```http
# Get all users
GET http://localhost:5070/users
Authorization: secret-appsettings-token-abc123

# Create new user
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

The application uses three middleware components in this order:

1. `Global Exception Handling` - Catches and logs unhandled exceptions.  
2. `Authentication` - Validates tokens for protected endpoints.  
3. `Request/Response Logging` - Logs incoming requests and outgoing responses.

## Configuration

- `AuthToken` (appsettings.json): example value `secret-appsettings-token-abc123` (fallback `mysecret123`)  
- `Base URL`: `http://localhost:5070`  
- `MAX_AGE`: `150`  
- `MAX_USERNAME_LENGTH`: `100`

## Validation Rules

- `username`: required, 1–100 characters  
- `email`: required, valid email format  
- `userAge`: integer between `0` and `150` inclusive

Validation failures return `400 Bad Request` with structured JSON describing errors.

## Project Structure

```
UserManagementAPI/
├── Program.cs                  # Main application with middleware and endpoints
├── Models/
│   └── User.cs                 # User model class
├── TestRequests.http           # Comprehensive test cases
├── README.md                   # This file
└── TEST-DOCUMENTATION.md       # Test coverage documentation
```

## Sample Data

The API starts with three pre-loaded users:

1. `Alice` (alice@example.com, Age: 30)  
2. `Bob` (bob@example.com, Age: 25)  
3. `Charlie` (charlie@example.com, Age: 35)

IDs start from `1`; new users are assigned incrementing IDs.

## Error Responses

- `400 Bad Request` — validation errors (JSON)  
- `401 Unauthorized` — missing/invalid token  
- `404 Not Found` — resource not found  
- `500 Internal Server Error` — centralized error handling

Examples:

Validation error:
```json
{ "error": "Username is required and cannot be empty" }
```

Not found:
```json
{ "error": "User with ID 999 not found" }
```

Server error (development mode may include details):
```json
{ "error": "An internal server error occurred", "details": "..." }
```

## Thread Safety

- Uses `ConcurrentDictionary<int, User>` for thread-safe storage  
- Uses `Interlocked.Increment()` for atomic ID generation

## Development & Extensibility

- Add middleware classes in `Middleware/` and register them in `Program.cs`.  
- Add or modify endpoints in `Program.cs` (minimal APIs) or `Controllers/` (if using controllers).  
- Implement new persistence in `Repositories/` and register implementations for `IUserRepository` in DI.  
- Update validation logic in the validation helper(s).

## Testing

- `TestRequests.http` includes 30+ scenarios (authentication, validation, CRUD, edge cases).  
- Use VS Code REST Client or curl to run tests.

## License

Educational / demo code.
