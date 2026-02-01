## Employee Manager

Employee Manager is a small full-stack system for managing employees, their
roles, and reporting relationships. It provides authentication, CRUD operations,
and a manager assignment flow to keep teams organized.

### Core Rules (High-Level)

- **Creation rules**: emails and document numbers must be unique; phones must
  have 9 digits; passwords must meet strong complexity requirements.
- **Access rules**: Directors can see and manage all employees. Non-directors
  can only see and manage employees where they are set as manager.
- **Soft delete**: deleting an employee deactivates the record instead of
  removing it.

### Running with Containers

The project is containerized with Docker:
- **API** (ASP.NET 8) runs in one container.
- **Frontend** (Vite build served by Nginx) runs in another container.
- **Database** (Postgres) runs in a third container.

Start everything with Docker Compose:
```bash
docker compose up --build
```

Default ports:
- API: `http://localhost:8080`
- Frontend: `http://localhost:5173`
- PgAdmin: `http://localhost:5050`

The API applies migrations automatically on startup (when seed is enabled), so
the database schema is created/updated on first run.

## Architecture Overview

This project uses a layered, clean-architecture-inspired design with clear
separation of concerns across the API, Application, Domain, and Infrastructure
layers. The goal is to keep business rules independent of delivery mechanisms
and data access, making the system easier to test, evolve, and maintain.

### Architectural Style

- **Layered architecture with clean boundaries**: Each layer has a single
  responsibility and depends only on the layers inside it. The outer layers
  (API, Infrastructure) depend on the inner layers (Application, Domain), not
  the other way around.
- **Domain-centric**: The Domain layer models business rules and invariants.
  It does not depend on any framework or persistence concerns.
- **Use-case driven**: The Application layer exposes explicit use cases that
  orchestrate domain objects and repositories to fulfill business actions.

### Layers and Their Responsibilities

#### 1) Domain (`EmployeeManager.Domain`)

This is the core of the business. It contains:

- **Entities and value objects** such as `Employee` and `Phone`.
- **Business invariants and validations** (e.g., adult age check, phone format,
  email/doc validation, self-manager constraint).
- **Lifecycle rules** (e.g., activation/deactivation, audit fields).

The domain must remain independent so it can be unit tested without any
framework dependencies.

#### 2) Application (`EmployeeManager.Application`)

This layer implements the business use cases. It is responsible for:

- **Use cases** like `CreateEmployee`, `UpdateEmployee`, `DeleteEmployee`,
  `Login`, `ListEmployees`, and `SearchEmployees`.
- **Application-level validation and orchestration**, such as checking
  uniqueness (email/doc/phone) before creating or updating.
- **Abstractions/Ports** that define required behaviors, like
  `IEmployeeRepository`, `IPasswordHasher`, and `IAccessTokenGenerator`.

Why this separation matters:
  - Use cases are small, explicit, and testable in isolation.
  - Business flow is centralized here, not spread across controllers or
    infrastructure classes.
  - Dependencies on frameworks are inverted through interfaces, allowing
    Infrastructure to plug in concrete implementations.

#### 3) Infrastructure (`EmployeeManager.Infrastructure`)

This layer provides concrete implementations for interfaces defined in the
Application layer. It includes:

- **EF Core repositories** (e.g., `EmployeeRepository`) that implement
  `IEmployeeRepository`.
- **Database models and mappings** (`EmployeeEntity`, `AppDbContext`).
- **Security adapters** (e.g., `PasswordHasherAdapter`) that implement
  `IPasswordHasher`.

Infrastructure can be replaced without changing the Domain or Application
layers as long as it honors the contracts (ports).

#### 4) API (`EmployeeManager.Api`)

This is the delivery mechanism of the application. It includes:

- **Controllers** that translate HTTP requests into use case calls and return
  DTOs.
- **Contracts/DTOs** that define the boundary between external clients and the
  application.
- **Middleware** for cross-cutting concerns like error handling.
- **Auth configuration** (JWT) and dependency injection wiring.

Controllers are intentionally thin. They should not contain business rules,
only composition and I/O concerns.

### Why Separate Controllers, Use Cases, Commands, and DTOs

This project uses a command-style approach in the Application layer:

- **Commands** (e.g., `CreateEmployeeCommand`, `UpdateEmployeeCommand`) model
  a single business operation with explicit inputs. This makes use cases
  self-documenting and stable.
- **Use cases** handle the orchestration: validation, repository access, domain
  construction, and persistence.
- **Controllers** only adapt transport input to command objects and trigger
  use cases; they do not implement business rules.
- **DTOs/Contracts** decouple the external API from internal domain models.
  That gives flexibility to evolve the domain without breaking the API.

This separation improves:
  - **Testability**: Use cases and domain logic can be unit tested without HTTP
    or database dependencies.
  - **Maintainability**: Business rules live in one place and remain consistent.
  - **Flexibility**: Switching persistence or delivery mechanisms is easier.
  - **Clarity**: Each class has a single, clear responsibility.

### Cross-Cutting Concerns

- **Logging** is performed in use cases and repositories to track key domain
  events and persistence operations.
- **Error handling** is centralized in API middleware to ensure consistent
  responses.
- **Authentication/Authorization** is handled at the API boundary, while the
  Application layer enforces business rules.

### Summary

The architecture aims to keep business logic consistent, explicit, and
framework-independent. The result is a system that is easier to understand,
test, and scale as requirements evolve.

## API Documentation

Base URL (local): `http://localhost:5172` (API)  
Authentication: Bearer JWT (`Authorization: Bearer <token>`)

All `/employees` endpoints require authentication. Authorization rules:
- **Director** can read/update/delete any employee.
- **Non-director** can only read/update/delete employees where
  `manager_employee_id` equals their own user id.
- **Non-director** cannot delete themselves.

### Auth

#### POST `/auth/login`
Authenticate and return an access token.

**Request body**
```json
{
  "email": "director@local.dev",
  "password": "Admin#12345678"
}
```

**Response 200**
```json
{
  "accessToken": "eyJhbGciOi..."
}
```

**Errors**
- `400` if payload is invalid
- `401/400` if credentials are invalid (wrapped by exception handler)

### Employees

#### POST `/employees`
Create a new employee.

**Request body**
```json
{
  "firstName": "Ana",
  "lastName": "Silva",
  "email": "ana@ex.com",
  "docNumber": "12345678901",
  "birthDate": "1990-01-01",
  "role": 1,
  "phones": [
    { "number": "999991111", "type": "mobile" },
    { "number": "333322222", "type": "home" }
  ],
  "password": "StrongPass1!",
  "managerEmployeeId": "88f41b26-1b0b-472c-b5bd-34722621d839"
}
```

Notes:
- `managerEmployeeId` is optional; if omitted, the creator becomes the manager.
- Password must have at least 8 chars, upper, lower, number, symbol.
- Phone numbers must be **9 digits only**.
- `docNumber` must contain only digits.

**Response 201**
```json
{
  "id": "f2d4c9c5-7bd0-4e64-9e3a-4a4a2ef3f7c3"
}
```

#### GET `/employees`
List employees (paginated).

**Query params**
- `skip` (default 0)
- `take` (default 20, max 100)

**Response 200**
```json
[
  {
    "id": "f2d4c9c5-7bd0-4e64-9e3a-4a4a2ef3f7c3",
    "firstName": "Ana",
    "lastName": "Silva",
    "email": "ana@ex.com",
    "docNumber": "12345678901",
    "birthDate": "1990-01-01",
    "role": 1,
    "phones": [
      { "number": "999991111", "type": "mobile" },
      { "number": "333322222", "type": "home" }
    ],
    "managerEmployeeId": "88f41b26-1b0b-472c-b5bd-34722621d839"
  }
]
```

#### GET `/employees/{id}`
Get a single employee by id.

**Response 200**
```json
{
  "id": "f2d4c9c5-7bd0-4e64-9e3a-4a4a2ef3f7c3",
  "firstName": "Ana",
  "lastName": "Silva",
  "email": "ana@ex.com",
  "docNumber": "12345678901",
  "birthDate": "1990-01-01",
  "role": 1,
  "phones": [
    { "number": "999991111", "type": "mobile" },
    { "number": "333322222", "type": "home" }
  ],
  "managerEmployeeId": "88f41b26-1b0b-472c-b5bd-34722621d839"
}
```

#### PUT `/employees/{id}`
Update an employee.

**Request body**
```json
{
  "firstName": "Ana",
  "lastName": "Silva",
  "email": "ana@ex.com",
  "docNumber": "12345678901",
  "birthDate": "1990-01-01",
  "role": 1,
  "phones": [
    { "number": "999991111", "type": "mobile" },
    { "number": "333322222", "type": "home" }
  ],
  "managerEmployeeId": "88f41b26-1b0b-472c-b5bd-34722621d839",
  "password": "NewStrong1!"
}
```

Notes:
- `password` is optional. If omitted or empty, the current password is kept.

**Response 204**

#### DELETE `/employees/{id}`
Soft-delete an employee (sets `is_active = false`).

**Response 204**

#### GET `/employees/search?q=...`
Search employees (used by the manager lookup).

**Query params**
- `q` (minimum 2 chars)
- `take` (default 10, max 20)

**Response 200**
```json
[
  {
    "id": "f2d4c9c5-7bd0-4e64-9e3a-4a4a2ef3f7c3",
    "name": "Ana Silva",
    "email": "ana@ex.com"
  }
]
```

### Error Format

The API returns JSON error responses in the form:
```json
{
  "message": "Human readable error message"
}
```
