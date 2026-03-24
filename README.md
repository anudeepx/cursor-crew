# Retail Ordering Website

Full-stack retail ordering application with:

- ASP.NET Core Web API (.NET 9)
- Entity Framework Core with SQL Server
- JWT-based authentication and role-based authorization
- Angular frontend

## Project Structure

- `Controllers/` API endpoints for auth, products, cart, orders, payments, inventory, users, categories, and brands
- `Data/` EF Core `AppDbContext` and design-time factory
- `Models/` Domain models (`User`, `Product`, `Order`, etc.)
- `DTOs/` Request and response contracts
- `Services/` JWT generation, user role resolution, in-memory cart/payment stores
- `Validators/` FluentValidation validators
- `Migrations/` EF Core migrations
- `frontend/` Angular app

## Tech Stack

### Backend

- .NET 9 Web API
- Entity Framework Core 9 (SQL Server provider)
- FluentValidation
- JWT Bearer Authentication
- Swagger/OpenAPI
- ASP.NET Core rate limiting and health checks

### Frontend

- Angular 21
- TypeScript 5.9
- RxJS

## Prerequisites

Install the following tools:

- .NET SDK 9.x
- SQL Server LocalDB (or SQL Server instance)
- Node.js 20+ and npm
- Angular CLI (optional globally; local npm scripts are enough)

## Configuration

Backend settings are in `appsettings.json`.

Important keys:

- `ConnectionStrings:DefaultConnection`
- `Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience`, `Jwt:ExpirationMinutes`
- `Auth:AdminEmails` (users with emails in this list are treated as Admin)

Example default connection string uses LocalDB:

`Server=(localdb)\\MSSQLLocalDB;Database=RetailOrderingDb;Trusted_Connection=True;MultipleActiveResultSets=true`

## Backend Setup and Run

From repository root:

```bash
dotnet restore
dotnet ef database update
dotnet run
```

Useful URLs after launch:

- Swagger UI: `https://localhost:xxxx/swagger`
- Health check: `https://localhost:xxxx/health`

Use the exact port shown in terminal output or `Properties/launchSettings.json`.

## Frontend Setup and Run

From `frontend/`:

```bash
npm install
npm start
```

App runs at:

- `http://localhost:4200`

CORS is configured in backend for:

- `http://localhost:4200`
- `https://localhost:4200`

## Authentication and Roles

Auth endpoints:

- `POST /api/auth/register`
- `POST /api/auth/login`
- `POST /api/auth/logout` (requires token)
- `GET /api/auth/me` (requires token)

After login/register, include JWT in protected requests:

`Authorization: Bearer <token>`

Role behavior:

- `Admin` role is assigned by email match in `Auth:AdminEmails`
- All other users are `User`

## Core API Overview

### Public or user-accessible endpoints

- `GET /api/products`, `GET /api/products/{id}`
- `GET /api/categories`
- `GET /api/brands`
- Cart: `GET /api/cart`, `POST /api/cart/add`, `PUT /api/cart/update`, `DELETE /api/cart/remove/{productId}`, `DELETE /api/cart/clear`
- Orders: place/list/get status and details (authorization checks apply)
- Payments: create/verify/get by order (authorization checks apply)

### Admin-only endpoints

- Products: create/update/delete
- Categories: create/update/delete
- Brands: create/update/delete
- Inventory: list/update
- Users: list/get/update/delete
- Orders: list all, update/track status

Use Swagger for request/response schemas and to test secured endpoints.

## Database and Migrations

Current migrations are under `Migrations/`.

Common commands:

```bash
dotnet ef migrations add <MigrationName>
dotnet ef database update
```

If `dotnet ef` is unavailable, install tool:

```bash
dotnet tool install --global dotnet-ef
```

## Development Notes

- Cart and payment stores are in-memory services (`Singleton`), suitable for development/demo scenarios.
- Rate limiting is enabled with a fixed window policy (100 requests per minute).
- Request logging middleware is enabled globally.

## Quick Start

1. Configure database and JWT settings in `appsettings.json`.
2. Run backend from root with `dotnet run`.
3. Run frontend from `frontend/` with `npm start`.
4. Register a user via UI or Swagger.
5. Add that user email to `Auth:AdminEmails` if admin access is needed.

## Troubleshooting

- 401 Unauthorized:
  - Ensure JWT is present in `Authorization` header.
  - Verify token issuer/audience/key match `appsettings.json`.
- CORS errors:
  - Ensure frontend runs on `http://localhost:4200` or `https://localhost:4200`.
- Database connection issues:
  - Confirm SQL Server/LocalDB is available.
  - Validate `ConnectionStrings:DefaultConnection`.
- Admin endpoints return 403:
  - Confirm user email exists in `Auth:AdminEmails`.

## License

No license file is currently included in this repository.
