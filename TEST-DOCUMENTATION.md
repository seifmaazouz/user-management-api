# Test Documentation - User Management API
## Comprehensive Test Coverage Report

### Test Suite Overview
The `TestRequests.http` file contains **30** comprehensive test scenarios covering all aspects of the User Management API, including authentication middleware, basic functionality, edge cases, error handling, and global exception handling. Tests use `@validToken = secret-appsettings-token-abc123` by default (see Authentication section).

---

## Test Categories

### 1. Public Endpoints Tests (Tests 1-2)
- **Test 1:** Root endpoint (`GET /`) — no authentication required  
- **Test 2:** Error endpoint (`GET /error`) — triggers global exception handler

**Purpose:** Verify public endpoints work without authentication and global exception handling functions correctly.

---

### 2. Authentication Tests (Tests 3-5)
- **Test 3:** `GET /api/users` without token (should fail with `401`)  
- **Test 4:** `GET /api/users` with invalid token (should fail with `401`)  
- **Test 5:** `GET /api/users` with valid token (should succeed)

**Purpose:** Verify authentication middleware properly validates tokens and protects endpoints.

**Token source:** appsettings.json `"AuthToken": "secret-appsettings-token-abc123"` (if not present the app falls back to `mysecret123`).  
**Header format:** `Authorization: secret-appsettings-token-abc123` (or the configured token)

---

### 3. GET Operations Tests (Tests 6-8)
- **Test 6:** `GET /api/users/1` with valid token and existing user  
- **Test 7:** `GET /api/users/999` with valid token for non-existing user (`404`)  
- **Test 8:** `GET /api/users/1` without token (`401`)

**Purpose:** Test retrieval operations with various authentication and data scenarios.

---

### 4. User Creation Tests (Tests 9-15)
- **Test 9:** `POST /api/users` with valid token and valid data  
- **Test 10:** `POST /api/users` without token (`401`)  
- **Test 11:** `POST /api/users` with empty `username` (validation error)  
- **Test 12:** `POST /api/users` with invalid `email` format (validation error)  
- **Test 13:** `POST /api/users` with negative `userAge` (validation error)  
- **Test 14:** `POST /api/users` with `userAge` too high (validation error)  
- **Test 15:** `POST /api/users` with `username` > 100 chars (validation error)

**Purpose:** Verify creation, authentication, and input validation.

---

### 5. User Update Tests (Tests 16-19)
- **Test 16:** `PUT /api/users/1` valid token and valid data  
- **Test 17:** `PUT /api/users/999` valid token (should return `404`)  
- **Test 18:** `PUT /api/users/2` no token (`401`)  
- **Test 19:** `PUT /api/users/2` valid token but invalid data (validation error)

**Purpose:** Test update flows with auth and validation.

---

### 6. User Deletion Tests (Tests 20-22)
- **Test 20:** `DELETE /api/users/3` valid token (should succeed)  
- **Test 21:** `DELETE /api/users/999` valid token (should return `404`)  
- **Test 22:** `DELETE /api/users/2` no token (`401`)

**Purpose:** Verify deletion behavior and access control.

---

### 7. Edge Cases & Boundary Tests (Tests 23-25)
- **Test 23:** `POST /api/users` with `userAge = 0` (minimum valid)  
- **Test 24:** `POST /api/users` with `userAge = 150` (maximum valid)  
- **Test 25:** `POST /api/users` with complex valid email

**Purpose:** Validate boundary conditions and email parsing.

---

### 8. Stress & State Tests (Tests 26-30)
- **Test 26:** `GET /api/users` after operations (verify state)  
- **Tests 27-29:** Rapid `POST /api/users` requests (concurrency / stress)  
- **Test 30:** Final `GET /api/users` state check

**Purpose:** Verify consistency under rapid requests and final state.

---

## Middleware Pipeline Testing

### Exception Handling Middleware
- **Test 2:** `/error` endpoint triggers the global handler  
- **Expected Response:**
```json
{
  "error": "An internal server error occurred",
  "details": "This is a test exception to trigger the global error handler."
}
```

### Authentication Middleware
- **Protected endpoints:** all `/api/users/*` endpoints require authentication  
- **Public endpoints:** `/` and `/error` bypass authentication  
- **Valid token (default test value):** `secret-appsettings-token-abc123`  
- **Invalid/missing token response:** `401 Unauthorized` (plain text `"Unauthorized"` by default)

### Logging Middleware
- Logs request method and path, and response status code for all requests.  
- Console example:
```
Request Method: GET, Path: /api/users
Response Status: 200
```

---

## Test Execution Guidelines

### Prerequisites
1. Install REST Client extension in VS Code  
2. Start API with:
```bash
dotnet run
```
3. API default base URL:
```
http://localhost:5070
```

### Running Tests
1. Open `TestRequests.http` in VS Code  
2. Verify variables at top of file:
```
@baseUrl = http://localhost:5070
@validToken = secret-appsettings-token-abc123
@invalidToken = wrongtoken
```
3. Click "Send Request" above each test and verify responses.

### Expected Response Patterns

#### Success Responses
- `200 OK` — successful GET, PUT  
- `201 Created` — successful POST  
- `204 No Content` — successful DELETE

#### Authentication Errors
- `401 Unauthorized` — missing or invalid token  
- Response body: `"Unauthorized"`

#### Validation Errors
- `400 Bad Request` — input validation failures  
- Response format:
```json
{ "error": "Specific validation error message" }
```

#### Not Found Errors
- `404 Not Found` — resource doesn't exist  
- Response format:
```json
{ "error": "User with ID {id} not found" }
```

#### Server Errors
- `500 Internal Server Error` — unhandled exceptions  
- Response format:
```json
{
  "error": "An internal server error occurred",
  "details": "Error details (development mode only)"
}
```

---

## Authentication Test Scenarios

### Valid Authentication
```http
GET /api/users
Authorization: secret-appsettings-token-abc123
Expected: 200 OK with user data
```

### Missing Authentication
```http
GET /api/users
Expected: 401 Unauthorized
```

### Invalid Authentication
```http
GET /api/users
Authorization: wrongtoken
Expected: 401 Unauthorized
```

### Public Endpoint (No Auth Required)
```http
GET /
Expected: 200 OK
```

---

## Validation Test Examples

### Username Validation
```json
// Empty username
{ "error": "Username is required and cannot be empty" }

// Username too long
{ "error": "Username cannot exceed 100 characters" }
```

### Email Validation
```json
// Missing email
{ "error": "Email is required and cannot be empty" }

// Invalid format
{ "error": "Email format is invalid" }
```

### Age Validation
```json
// Negative age
{ "error": "Age cannot be negative" }

// Age too high
{ "error": "Age cannot exceed 150" }
```

---

## Test Coverage Metrics

### Middleware Coverage
- ✅ Exception handling middleware tested  
- ✅ Authentication middleware tested  
- ✅ Request/response logging verified

### Functional Coverage
- ✅ CRUD operations covered  
- ✅ Authentication scenarios tested  
- ✅ Validation rules verified  
- ✅ Error scenarios included  
- ✅ Edge cases tested

### Security & Error Handling
- ✅ Protected endpoints require authentication  
- ✅ Invalid tokens rejected  
- ✅ Missing tokens handled  
- ✅ Resource not found handling verified  
- ✅ Global exception middleware tested

---

## Quality Assurance Notes

- Tests are sequentially numbered and grouped by functionality.  
- Variables at the top of `TestRequests.http` centralize configuration.  
- Tests are independent and designed for repeatable execution.  
- Adding tests: append new requests to `TestRequests.http` and reference `@baseUrl`/`@validToken`.

---

**Result:** Complete test suite ensuring the `User Management API`'s middleware, authentication, validation, and CRUD surface behave as expected under functional, edge-case, and stress scenarios.
