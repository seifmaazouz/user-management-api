# User Management API

A concise, production-oriented REST API for managing users built with ASP.NET Core. The project demonstrates clear separation of concerns, middleware-driven cross-cutting behavior, simple token authentication, logging, validation and thread-safe in-memory storage.

## Highlights
- Clean, testable architecture
- CRUD operations for users
- Token-based authentication middleware
- Centralized exception handling
- Request/response logging middleware
- Input validation with consistent error responses
- Thread-safe in-memory repository for demo and testing

## Separation of Concerns (Architecture)
The project is organized to separate responsibilities and make components independently testable and replaceable:

- Middleware
  - All cross-cutting concerns are implemented as middleware components (authentication, global exception handling, request/response logging). Middleware lives in a dedicated folder and is registered in the pipeline in `Program.cs`.
- Controllers / Endpoints
  - HTTP surface is implemented as controllers (or minimal endpoints) contained in a Controllers folder. Controllers are thin and delegate business logic to repository/service abstractions.
- Interfaces
  - Contracts (for repositories, services, etc.) live in an `Interfaces` folder. This decouples implementation from usage and enables easy mocking in tests.
- Repositories
  - Data access implementations (e.g., `InMemoryUserRepository`) are located in a `Repositories` folder and implement repository interfaces. This keeps persistence details out of controllers.
- Models / DTOs
  - Request/response models and domain models are stored in `Models`. DTOs (if present) separate transport shapes from domain entities.
- Program.cs / Composition Root
  - Dependency injection, middleware registration, and route mapping are configured in `Program.cs`. The composition root wires interfaces to concrete implementations.

This structure enforces single-responsibility and makes it straightforward to replace the in-memory store with a database-backed repository or add new middleware without modifying controllers.

## Quick Start
Prerequisites: .NET 8.0+, VS Code (REST Client recommended)

1. Run:
   dotnet run

2. Open `TestRequests.http` in VS Code and send requests. Default base URL: `http://localhost:5070`

## API Endpoints
Protected endpoints require the `Authorization` header with a valid token.

- GET /api/users — Get all users
- GET /api/users/{id} — Get user by id
- POST /api/users — Create user
- PUT /api/users/{id} — Update user
- DELETE /api/users/{id} — Delete user
- GET / — Root health check
- GET /error — Trigger error for testing global exception handling

## Authentication
Use the `Authorization` header:
Authorization: mysecret123

Unauthorized or missing tokens return 401.

## Validation Rules
- username: required, 1–100 chars
- email: required, valid email format
- userAge: integer, 0–150 inclusive

Validation errors return 400 with a JSON error message.

## Thread Safety
- Uses ConcurrentDictionary<int, User> for storage
- Uses Interlocked.Increment for atomic ID generation

## Testing
- `TestRequests.http` includes end-to-end test cases for authentication, validation, CRUD operations and edge cases.
- Interfaces and repository implementations are designed for unit testing (mockable via DI).

## Extending the Project
- Add middleware in the `Middleware` folder and register in `Program.cs`
- Add new controllers under `Controllers`
- Add new persistence implementations under `Repositories` and register via their interface
- Adjust validation and constants in `Program.cs` or dedicated configuration

## License

This project is for educational purposes.
