# PharmacyApp

Backend-focused pharmacy management platform built as a personal pet project to practice real-world .NET development.

The backend is built with ASP.NET Core Web API, Entity Framework Core, PostgreSQL, ASP.NET Core Identity, JWT authentication, and Clean Architecture. A supporting Next.js/React frontend is included to demonstrate the main application flows.

## Project Overview

PharmacyApp simulates a pharmacy management system with customer-facing features and admin functionality.

The backend API includes authentication, authorization, role-based access control, product catalog, shopping cart, order processing, discounts, promo codes, bonus loyalty system, reviews, email notifications, and admin operations.

The frontend provides an interface for catalog browsing, product details, cart, checkout, user profile, orders, promotions, content pages, and admin dashboard.

## Demo

The current demo is available at:

- https://pharmacyfrontend-production-d550.up.railway.app/

## My Focus

The main focus of this project was backend development with .NET:

- REST API development with ASP.NET Core Web API
- Clean Architecture and separation of responsibilities
- Entity Framework Core with PostgreSQL
- Authentication and authorization with JWT and ASP.NET Core Identity
- Role-based access control
- Business logic for products, carts, orders, discounts, promo codes, and bonus points
- Admin-related functionality
- Docker-based local database setup

The frontend was added as a supporting UI to make the project easier to present.

## Key Features

- JWT authentication with refresh tokens
- ASP.NET Core Identity integration
- Role-based access control for Admin, Pharmacist, Manager, and Customer
- Product catalog with categories, stock tracking, reviews, and ratings
- Shopping cart and checkout flow
- Order lifecycle and status tracking
- Category and product discounts
- Promo code validation logic
- Bonus points accrual and redemption
- Email confirmation, password reset, and order notifications
- Admin dashboard for managing products, categories, discounts, promo codes, reviews, users, and content pages

## Technology Stack

- **Backend**: C# 13, .NET 9, ASP.NET Core Web API
- **Frontend**: Next.js 16, React 19, TypeScript, Tailwind CSS 4, Zustand
- **Architecture**: Clean Architecture on the backend, App Router + layered frontend modules
- **ORM**: Entity Framework Core with PostgreSQL
- **Infrastructure**: Docker, MailKit, FluentValidation

## Project Structure

- `src/PharmacyApp.Domain`: domain entities and business rules
- `src/PharmacyApp.Application`: application services, DTOs, interfaces, mappings
- `src/PharmacyApp.Infrastructure`: EF Core, repositories, migrations, external integrations
- `src/PharmacyApp.Presentation`: API controllers and presentation concerns
- `src/PharmacyApp.WebApi`: API host, middleware, configuration
- `frontend/src/app`: Next.js App Router pages and route entry points
- `frontend/src/widgets`: page-level and layout widgets
- `frontend/src/features`: user-facing feature modules
- `frontend/src/entities`: domain-oriented UI and data slices
- `frontend/src/shared`: API client, shared UI, stores, config, and types

## Architecture

The backend follows Clean Architecture principles:

```text
WebApi / Presentation
        ↓
Application
        ↓
Domain
        ↑
Infrastructure
```

Main layers:

- **Domain** — core entities and business rules
- **Application** — services, DTOs, interfaces, mappings, and validation
- **Infrastructure** — EF Core, repositories, migrations, and external integrations
- **Presentation** — API controllers
- **WebApi** — application host, middleware, dependency injection, and configuration

## Getting Started

### Prerequisites

- .NET 9 SDK
- Node.js LTS
- Docker Desktop

### Clone the repository

```bash
git clone https://github.com/djavuch/pharmacy_app.git
cd pharmacy_app
```

### Start PostgreSQL with Docker

Create a `.env` file in the repository root:

```env
POSTGRES_USER=postgres
POSTGRES_PASSWORD=your_password
POSTGRES_DB=PharmacyAppDb
PGADMIN_EMAIL=admin@example.com
PGADMIN_PASSWORD=admin_password
```

Start PostgreSQL and pgAdmin:

```bash
docker compose up -d postgresdb pgadmin
```

pgAdmin:

```text
http://localhost:5050
```

### Configure the backend

Set the connection string in `src/PharmacyApp.WebApi/appsettings.Development.json` or use .NET User Secrets.

Example:

```json
{
  "ConnectionStrings": {
    "PharmacyAppConnection": "Host=localhost;Port=5432;Database=PharmacyAppDb;Username=postgres;Password=your_password"
  }
}
```

### Apply migrations

```bash
dotnet ef database update \
  --project src/PharmacyApp.Infrastructure \
  --startup-project src/PharmacyApp.WebApi
```

### Run the backend

```bash
dotnet run --project src/PharmacyApp.WebApi
```

API URLs:

```text
https://localhost:7072
http://localhost:5184
```

### Run the frontend

Create `frontend/.env.local`:

```env
NEXT_PUBLIC_API_URL=https://localhost:7072
```

Then run:

```bash
cd frontend
npm install
npm run dev
```

Frontend URL:

```text
https://localhost:3000
```

## Demo Credentials

For local development, the application can create an initial admin account during the first startup.

```text
Email:    admin@pharmacy.com
Password: Admin@123456
```

After the first successful startup, admin bootstrap should be disabled in configuration.

## What I Learned

During this project, I practiced:

- Designing REST APIs with ASP.NET Core Web API
- Structuring a backend application using Clean Architecture
- Working with Entity Framework Core and PostgreSQL
- Implementing authentication, refresh tokens, and role-based authorization
- Modeling business logic for orders, discounts, promo codes, and bonus points
- Connecting a frontend UI to a .NET backend API
