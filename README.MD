# PharmacyApp

PharmacyApp is a pharmacy management platform with an **ASP.NET Core 9 backend** and a **Next.js frontend** in one repository. The project includes customer flows, account management, promotions, bonus loyalty features, and an admin panel for operational work.

## Project Overview

The repository contains two main parts:

- **Backend API**: authentication, catalog, cart, orders, discounts, bonus program, reviews, email notifications, and admin operations
- **Frontend app**: catalog browsing, product pages, cart, checkout, profile, orders, promotions, content pages, and admin dashboard

## Demo

The current demo is available at:

- https://pharmacyfrontend-production-d550.up.railway.app/

## Key Features

### Authentication & Authorization
- JWT-based authentication with refresh token support
- ASP.NET Core Identity integration
- Role-based access control for Admin, Pharmacist, Manager, and Customer
- Email confirmation, password reset, and password change flows
- Automatic admin bootstrap for first-time setup

### Shopping & Orders
- Shopping cart for authenticated users
- Cart refresh after sign-in and sign-out
- Full order lifecycle from checkout to cancellation
- Order status tracking and email notifications
- Optimistic concurrency via `RowVersion`

### Pricing, Promotions, Loyalty
- Category-level and product-level discounts
- Promo codes with activation and validation
- Bonus points accrual and redemption
- Bonus transaction history and configurable settings

### Catalog & Content
- Product catalog with categories and stock tracking
- Reviews and ratings
- Wishlist and saved addresses
- Promotion pages and editable content pages

### Administration
- Admin dashboard
- Product, category, discount, promo code, review, and user management
- Bonus settings and content page management

## Technology Stack

- **Backend**: C# 13, .NET 9, ASP.NET Core Web API
- **Frontend**: Next.js 16, React 19, TypeScript, Tailwind CSS 4, Zustand
- **Architecture**: Clean Architecture on the backend, App Router + layered frontend modules
- **ORM**: Entity Framework Core with PostgreSQL
- **Infrastructure**: Docker, Docker Compose, MailKit, FluentValidation

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

## Getting Started

### 1. Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js LTS](https://nodejs.org/)
- [PostgreSQL](https://www.postgresql.org/download/) or Docker
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) for containerized runs

### 2. Clone the repository

```bash
git clone https://github.com/djavuch/PharmacyApp.git
cd PharmacyApp
```

### 3. Configure the backend

Set local development settings in `src/PharmacyApp.WebApi/appsettings.Development.json` or via User Secrets.

Database connection string:

```json
{
  "ConnectionStrings": {
    "PharmacyAppConnection": "Host=localhost;Port=5432;Database=PharmacyAppDb;Username=postgres;Password=your_password"
  }
}
```

Admin bootstrap for the first run:

```json
{
  "AdminBootstrap": {
    "Enabled": true,
    "Email": "admin@pharmacy.com",
    "Password": "Admin@123456",
    "FirstName": "System",
    "LastName": "Administrator"
  }
}
```

JWT settings:

```json
{
  "JwtSettings": {
    "SecretKey": "your-super-secret-key-at-least-32-characters-long-for-production",
    "Issuer": "PharmacyApp",
    "Audience": "PharmacyAppUsers",
    "ExpirationMinutes": 60
  }
}
```

### 4. Configure the frontend

Create `frontend/.env.local` from `frontend/.env.example`:

```env
NEXT_PUBLIC_API_URL=https://localhost:7072
```

Frontend notes:

- Do not add `/api` to `NEXT_PUBLIC_API_URL`. The client calls routes such as `/account/login`, `/store/products`, and `/admin/products` directly.
- The backend CORS config allows `https://localhost:3000` by default.
- The frontend dev script uses HTTPS via `next dev --experimental-https`.

### 5. Run the application locally

Apply backend migrations:

```bash
cd src/PharmacyApp.WebApi
dotnet ef database update --project ../PharmacyApp.Infrastructure
```

Run the backend API:

```bash
dotnet run --project src/PharmacyApp.WebApi
```

Run the frontend:

```bash
cd frontend
npm install
npm run dev
```

Development URLs:

- API HTTPS: `https://localhost:7072`
- API HTTP: `http://localhost:5184`
- Frontend: `https://localhost:3000`

### 6. Test the bootstrap admin account

```text
email: admin@pharmacy.com
password: Admin@123456
```

### 7. Disable admin bootstrap after first run

Set `AdminBootstrap:Enabled` to `false` after the first successful startup.

## Frontend Deployment

The frontend can be deployed to any platform that supports Next.js 16. At minimum, set:

```env
NEXT_PUBLIC_API_URL=https://<your-backend-domain>
```

Deployment notes:

- The backend must allow the deployed frontend origin in `Cors__AllowedOrigins__0`
- `Frontend__BaseUrl` should match the public frontend domain used in links and emails
- Keep frontend and backend on HTTPS to avoid mixed-content issues in browsers
