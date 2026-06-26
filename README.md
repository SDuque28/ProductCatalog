# Product Catalog Application

This repository contains a technical assessment for a Product Catalog application composed of an ASP.NET Core Web API backend and an Angular frontend. The current implementation reads product data from a TXT file, exposes it through REST endpoints, and consumes it from a standalone Angular application.

The solution was organized with a layered architecture and clean separation of responsibilities. Business rules live in the service layer, file access is isolated behind a repository abstraction, and the frontend consumes the API through dedicated services and feature-level state management. The goal is to keep the codebase simple, testable, and easy to extend later with a different persistence mechanism.

---

## Features

- Product listing
- Product details
- Search by name
- Stock filtering
- Server-side pagination
- Low-stock endpoint
- JWT authentication for protected backend endpoints
- User registration with TXT-based persistence
- Global exception handling
- TXT-based product data source
- Malformed TXT row handling
- Unit and integration tests for the backend
- Frontend unit tests
- Responsive Angular UI with loading, error, empty, and success states
- Frontend login flow with JWT persistence and protected routes
- Frontend registration flow for new users

---

## Architecture

The backend follows a layered architecture with dependency injection and clear abstractions:

```text
Controller
    ↓
Service Layer
    ↓
Repository Abstraction
    ↓
TXT Repository
    ↓
products.txt
```

- Controllers orchestrate HTTP requests and responses.
- Services contain application logic such as filtering, pagination, and stock-state calculation.
- Repository interfaces decouple the application from the concrete data source.
- The TXT repository reads and parses `products.txt` without leaking file-access details to upper layers.
- Dependency Injection wires controllers, services, and repositories through `Program.cs`.

The Repository Pattern was chosen so the current TXT source can be replaced later by a database-backed implementation without changing controllers or service contracts. This abstraction improves maintainability, testability, and future scalability.

---

## Project Structure

```text
ProductCatalog/
├── ProductCatalog.Api/
│   ├── Controllers/
│   ├── DTOs/
│   ├── Data/
│   ├── Exceptions/
│   ├── Interfaces/
│   ├── Middleware/
│   ├── Models/
│   ├── Repositories/
│   ├── Services/
│   ├── Program.cs
│   └── ProductCatalog.Api.csproj
├── ProductCatalog.Front/
│   ├── src/
│   │   ├── app/
│   │   │   ├── core/
│   │   │   ├── features/
│   │   │   ├── layout/
│   │   │   └── shared/
│   │   └── environments/
│   ├── angular.json
│   └── package.json
├── ProductCatalog.Tests/
│   ├── Controllers/
│   ├── Repositories/
│   ├── Services/
│   └── ProductCatalog.Tests.csproj
├── ProductCatalog.slnx
└── README.md
```

- `ProductCatalog.Api`: backend API, domain models, DTOs, repository, services, middleware, and TXT data source.
- `ProductCatalog.Front`: Angular frontend with standalone components, routing, shared UI states, and feature modules.
- `ProductCatalog.Tests`: backend test project with service, repository, and controller coverage.
- `ProductCatalog.slnx`: solution file including the API and test projects.

---

## Backend

### Technologies used

- ASP.NET Core Web API
- .NET 10
- Dependency Injection
- Repository Pattern
- Service Layer
- OpenAPI document generation in development
- Swagger UI with Bearer token support
- xUnit
- Moq
- FluentAssertions

### API Endpoints

| Method | Endpoint | Description |
|---------|----------|-------------|
| `POST` | `/api/auth/register` | Register a new user in the TXT-based user store |
| `POST` | `/api/auth/login` | Authenticate a registered user and obtain a JWT token |
| `GET` | `/api/products` | Retrieve a paginated product list with optional filters |
| `GET` | `/api/products/{id}` | Retrieve a single product by identifier |
| `GET` | `/api/products/low-stock` | Retrieve products with stock less than or equal to a threshold |

Authentication implemented: `Yes`

Protected endpoints:

- `GET /api/products`
- `GET /api/products/{id}`
- `GET /api/products/low-stock`

Public endpoint:

- `POST /api/auth/register`
- `POST /api/auth/login`

Registration request example:

```json
{
  "username": "pablo",
  "email": "pablo@example.com",
  "password": "Password123*",
  "confirmPassword": "Password123*"
}
```

Successful registration response example:

```json
{
  "username": "pablo",
  "email": "pablo@example.com",
  "createdAtUtc": "2026-01-01T12:00:00Z",
  "message": "User registered successfully."
}
```

Login request example:

```json
{
  "username": "admin",
  "password": "Admin123*"
}
```

Successful login response example:

```json
{
  "token": "<JWT_TOKEN>",
  "expiresAt": "2026-01-01T12:00:00Z",
  "tokenType": "Bearer"
}
```

Demo credentials:

- Username: `admin`
- Email: `admin@example.com`
- Password: `Admin123*`

The technical assessment seeds the demo admin user into `ProductCatalog.Api/Data/users.txt` when the file is missing or empty so the existing login flow keeps working.

To call protected endpoints, include the JWT in the `Authorization` header:

```http
Authorization: Bearer <TOKEN>
```

Supported query parameters for `GET /api/products`:

| Parameter | Type | Description |
|-----------|------|-------------|
| `name` | `string` | Partial product name search, case-insensitive |
| `inStock` | `boolean` | Filters products by stock availability |
| `page` | `int` | Page number, must be `>= 1` |
| `pageSize` | `int` | Page size, must be between `1` and `100` |

Example request:

```http
GET /api/products?name=mouse&inStock=true&page=1&pageSize=10
```

Low-stock example:

```http
GET /api/products/low-stock?threshold=5
```

Not implemented:

- Product creation
- Product update
- Product deletion
- Database persistence

---

## Frontend

The frontend is an Angular standalone application organized by `core`, `shared`, `layout`, and `features` layers.

- `core`: API service, shared models, and HTTP interceptor.
- `features/products`: product list page, product detail page, feature facades, routes, and models.
- `shared/components`: reusable loading, error, and empty-state components.
- `layout/components`: navbar, footer, and 404 page.

Main screens currently implemented:

- Login
- Register
- Product List
- Product Detail
- Page Not Found

Frontend routing:

- `/login`
- `/register`
- `/products`
- `/products/:id`
- `/**` for 404 handling

The Product List page uses server-side search, stock filtering, and pagination. The Product Detail page loads a single product by route parameter and handles loading, error, and not-found states.

### Frontend authentication

- Login route: `/login`
- Register route: `/register`
- Demo credentials:
  Username: `admin`
  Password: `Admin123*`
- Protected routes:
  `/products`
  `/products/:id`
- Token storage:
  `product_catalog_token`
  `product_catalog_token_expires_at`
  `product_catalog_token_type`
- Logout behavior:
  the header logout action clears the stored token and redirects back to `/login`
- Session behavior:
  refreshing the page keeps the session active while the stored token is still valid
  a `401 Unauthorized` response clears the token and returns the user to `/login`
- Manual flow:
  1. Register a user from `/register`
  2. Sign in from `/login` with the new credentials
  3. Access `/products` and `/products/:id`
  4. Logout from the header to clear the session

---

## Product Data Source

Product data is stored in `ProductCatalog.Api/Data/products.txt`.

Format:

- Delimiter: `|`
- First line: header row
- Expected columns: `IdProducto|NombreProducto|Descripcion|Valor|Stock`

Example:

```text
IdProducto|NombreProducto|Descripcion|Valor|Stock
1|Teclado mecanico|Teclado RGB switch azul|150000|12
2|Mouse inalambrico|Mouse ergonomico 2.4GHz|85000|0
```

Malformed record handling in the TXT repository:

- Empty lines are ignored.
- Rows with fewer than 5 columns are ignored.
- Rows with invalid `IdProducto`, `Valor`, or `Stock` are ignored.
- Rows with an empty `NombreProducto` are ignored.
- Empty `Descripcion` is allowed.
- Duplicate `IdProducto` values keep the first valid row and ignore later duplicates.
- Repository methods do not throw for malformed rows; exceptions are only expected when the file itself cannot be accessed.

## User Data Source

Registered users are stored in `ProductCatalog.Api/Data/users.txt`.

- The file is created automatically on startup or during auth operations if it does not already exist.
- The file is ignored by Git through `.gitignore`.
- Passwords are stored as SHA256 hashes, not in plain text.
- This hashing approach is acceptable for a technical assessment only. In production, stronger password hashing such as BCrypt, PBKDF2, or Argon2 should be used.

`users.txt` format:

```text
username|email|passwordHash|createdAtUtc
```

Example:

```text
admin|admin@example.com|<sha256_hash>|2026-01-01T12:00:00Z
```

---

## Error Handling

The API uses a global exception middleware to centralize unhandled exception processing and return consistent JSON responses.

Current mappings:

- `200 OK`
- `201 Created`
- `400 Bad Request`
- `401 Unauthorized`
- `403 Forbidden`
- `409 Conflict`
- `404 Not Found`
- `500 Internal Server Error`

Custom exceptions currently implemented:

- `BadRequestException`
- `ConflictException`
- `NotFoundException`

In production-style responses, internal exception details are not exposed to the client.

---

## Running the Project

### Backend

From the repository root:

```bash
dotnet restore ProductCatalog.slnx
dotnet run --project ProductCatalog.Api
```

Default backend URLs:

- `http://localhost:5000`
- `https://localhost:7105`

Swagger UI is available in development at:

- `https://localhost:7105/swagger`
- `http://localhost:5000/swagger`

JWT configuration and the demo user are stored in `ProductCatalog.Api/appsettings.json` for assessment purposes. In production, secrets must be stored securely through environment variables or a secret manager.

Example registration request:

```bash
curl -X POST https://localhost:7105/api/auth/register \
  -H "Content-Type: application/json" \
  -d "{\"username\":\"pablo\",\"email\":\"pablo@example.com\",\"password\":\"Password123*\",\"confirmPassword\":\"Password123*\"}"
```

Example login request:

```bash
curl -X POST https://localhost:7105/api/auth/login \
  -H "Content-Type: application/json" \
  -d "{\"username\":\"admin\",\"password\":\"Admin123*\"}"
```

Example protected request:

```bash
curl https://localhost:7105/api/products \
  -H "Authorization: Bearer <TOKEN>"
```

### Frontend

From `ProductCatalog.Front`:

```bash
npm install
npm run start
```

Default frontend URL:

- `http://localhost:4200`

The development environment is configured to call:

```text
http://localhost:5000/api
```

---

## Running Tests

### Backend tests

From the repository root:

```bash
dotnet test ProductCatalog.slnx
```

### Frontend tests

From `ProductCatalog.Front`:

```bash
npm test -- --watch=false
```

---

## Design Decisions

### 1. Repository Pattern

- Problem: The application needed to read product data from a TXT file without coupling the rest of the system to file access.
- Decision: Introduce `IProductRepository` with a TXT-based implementation.
- Benefit: The data source can be replaced later with a database repository without changing controllers or service consumers.

### 2. Service Layer

- Problem: Filtering, pagination, and stock-related behavior should not live in controllers.
- Decision: Implement `IProductService` and `ProductService` as the application layer.
- Benefit: Controllers remain thin, business rules stay centralized, and the logic is easier to test.

### 3. DTO-Based API Contracts

- Problem: Controllers should not expose domain entities directly.
- Decision: Use request and response DTOs for product list, product detail, pagination, query parameters, and errors.
- Benefit: Contracts remain explicit, stable, and independent from internal model changes.

### 4. Global Exception Middleware

- Problem: Repeating `try/catch` blocks in controllers would add noise and inconsistency.
- Decision: Centralize exception handling in middleware.
- Benefit: Error responses are standardized and controller actions stay focused on orchestration.

### 5. Frontend Feature Facades

- Problem: UI components needed loading, error, and success state management without mixing state orchestration with markup concerns.
- Decision: Keep API access in core services and manage feature state with product facades.
- Benefit: Components stay smaller, state transitions are easier to test, and server-driven behavior remains reusable.

### 6. TXT-Based User Registration

- Problem: The assessment required registration and login persistence without introducing a database.
- Decision: Store users in `Data/users.txt` behind `IUserRepository`, and hash passwords with a reusable SHA256 hasher.
- Benefit: Authentication stays simple, testable, and aligned with the layered backend structure while keeping real user data out of Git.

---

## Future Improvements

- Database persistence with a replaceable repository implementation
- CRUD endpoints for products
- Docker support
- CI/CD pipeline
- OpenAPI/Swagger UI enhancements
- Caching strategy for product queries
- Observability and structured monitoring

---

## Quality Attributes

- Maintainability: Responsibilities are split across controllers, services, repositories, DTOs, and shared UI components.
- Scalability: The repository abstraction allows the TXT source to be replaced with a database-backed implementation later.
- Testability: Backend layers are covered with service, repository, and controller tests; frontend product states also include unit tests.
- Modularity: The Angular application separates core infrastructure, shared UI, layout, and product feature concerns.

---

## Author

Author: `SDuque28`
