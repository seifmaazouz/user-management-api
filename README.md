# User Management API

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
   git clone <repository-url>
   cd UserManagementAPI
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

Use the `Authorization` header with the token:

```http
Authorization: mysecret123
```

### Example Requests

```http
# Get all users
GET http://localhost:5070/users
Authorization: mysecret123

# Create new user
POST http://localhost:5070/users
Authorization: mysecret123
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

1. **Global Exception Handling** - Catches and logs all exceptions
2. **Authentication** - Validates tokens for protected endpoints
3. **Request/Response Logging** - Logs all HTTP requests and responses

## Testing

The project includes comprehensive test cases in `TestRequests.http`:

- 30+ test scenarios
- Authentication tests
- CRUD operation tests
- Input validation tests
- Edge case testing
- Error handling verification

## Project Structure

```
UserManagementAPI/
├── Program.cs                  # Main application with middleware and endpoints
├── Models/
│   └── User.cs                # User model class
├── TestRequests.http          # Comprehensive test cases
├── README.md                  # This file
├── COPILOT-ASSISTANCE.md      # Copilot assistance documentation
└── TEST-DOCUMENTATION.md     # Test coverage documentation
```

## Sample Data

The API starts with three pre-loaded users:

1. Alice (alice@example.com, Age: 30)
2. Bob (bob@example.com, Age: 25)
3. Charlie (charlie@example.com, Age: 35)

## Error Responses

The API returns consistent JSON error responses:

```json
{
    "error": "Error description",
    "details": "Additional details (in development mode)"
}
```

Common HTTP status codes:
- `200` - Success
- `201` - Created
- `204` - No Content (delete success)
- `400` - Bad Request (validation errors)
- `401` - Unauthorized (invalid/missing token)
- `404` - Not Found
- `500` - Internal Server Error

## Development

To extend this API:

1. **Add new endpoints** in `Program.cs` after the existing ones
2. **Modify validation** in the `ValidateUser` method
3. **Update authentication** by changing the token validation logic
4. **Add new middleware** between existing middleware components

## Learning Objectives

This project demonstrates:

- ASP.NET Core minimal APIs
- Middleware pipeline implementation
- Authentication patterns
- Error handling strategies
- Input validation techniques
- Logging best practices
- RESTful API design
- HTTP testing methodologies

## License

This project is for educational purposes.
