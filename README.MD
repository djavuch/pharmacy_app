# PharmacyApp

A modern pharmacy management system with **ASP.NET Core 9.0 backend** and **Next.js frontend** in one repository.

## Project Overview
PharmacyApp is a robust RESTful API designed to handle pharmaceutical products, categories, shopping carts, bonus loyalty systems and customer reviews.

### Key Features
#### Authentication & Authorization
- **JWT-based Authentication**: Secure token-based authentication with refresh token support
- **ASP.NET Core Identity Integration**: User management with email confirmation and password reset
- **Role-Based Access Control**: Multi-role system (Admin, Pharmacist, Manager, Customer)
- **Email Confirmation**: Required email verification for enhanced security
- **Password Management**: Forgot password, reset password, and change password functionality
- **Automatic Admin Bootstrap**: First-time administrator setup via configuration

#### Shopping & Orders
- **Shopping Cart Management**: Guest and authenticated user carts with session handling
- **Cart Merging**: Automatic cart merge on login/logout for seamless user experience
- **Full Order Lifecycle**: From cart to checkout to order cancellation
- **Order Status Tracking**: Multiple order statuses (Pending, Processing, Shipped, Delivered, Cancelled)
- **Order Email Notifications**: Automated email updates for order status changes
- **Concurrency Control**: Race condition prevention using `RowVersion` timestamps

#### Pricing & Promotions
- **Dynamic Discount System**: Support for percentage and fixed amount discounts
- **Category & Product-Level Discounts**: Flexible discount assignment
- **Promo Codes**: Create, validate and apply promotional codes
- **Active Discount Management**: Time-based discount activation

#### Loyalty Program
- **Bonus Points System**: Automated accrual and deduction during purchases
- **Transaction History**: Detailed bonus transaction tracking
- **Configurable Bonus Settings**: Customizable earning and redemption rates
- **Bonus Account Management**: User-specific bonus accounts

#### Product Management
- **Complete Product Catalog**: Medications with detailed information
- **Category Filtering**: Organized product categorization
- **Product Reviews & Ratings**: Customer feedback system
- **Wishlist**: Save products for later purchase
- **Stock Management**: Inventory tracking and availability

#### User Management
- **User Profiles**: Personal information and address management
- **Multiple Address Support**: Save and manage multiple delivery addresses
- **Admin User Management**: Lock/unlock users, role assignment
- **User Filtering & Pagination**: Advanced user search and filtering

#### Security & Data Integrity
- **Email Confirmation Required**: EmailConfirmed policy for sensitive operations
- **Data Protection**: ASP.NET Core Data Protection for secure token persistence
- **Optimistic Concurrency**: RowVersion-based conflict resolution
- **Account Lockout**: Protection against brute force attacks

#### Email System
- **MailKit Integration**: Professional email sending capabilities
- **Background Email Queue**: Non-blocking email processing
- **Account Notifications**: Registration confirmation, password reset emails
- **Order Notifications**: Order confirmation and status update emails

## Technology Stack
- **Backend**: C# 13, .NET 9, ASP.NET Core Web API
- **Frontend**: Next.js 16, React 19, TypeScript
- **Architecture**: Clean Architecture (Domain, Application, Infrastructure, Presentation, WebApi)
- **ORM**: Entity Framework Core (PostgreSQL support)
- **Containerization**: Docker, Docker Compose
- **Tools**: FluentValidation, MailKit

## Project Structure
- `src/PharmacyApp.Domain`: Core business entities and domain logic.
- `src/PharmacyApp.Application`: Application services, interfaces, DTOs and mappings.
- `src/PharmacyApp.Infrastructure`: EF Core, repositories, external services, migrations.
- `src/PharmacyApp.Presentation`: API controllers and presentation-level concerns.
- `src/PharmacyApp.WebApi`: API host, middleware, app configuration.
- `frontend/`: Next.js client application.

## Getting Started

### 1. Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js LTS](https://nodejs.org/)
- [PostgreSQL](https://www.postgresql.org/download/) or use Docker
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (optional, for containerized deployment)

### 2. Clone the repository
````
git clone https://github.com/djavuch/PharmacyApp.git
cd PharmacyApp
````

### 3. Configure appsettings.Development.json with (or use User.secrets):
####   - Database connection string
```
{
  "ConnectionStrings": {
    "PharmacyAppConnection": "Host=localhost;Port=5432;Database=PharmacyAppDb;Username=postgres;Password=your_password"
  }
}
```
####   - AdminBootstrap settings (Enabled: true)
```
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
####    - JWT secret key
```
{
  "JwtSettings": {
    "SecretKey": "your-super-secret-key-at-least-32-characters-long-for-production",
    "Issuer": "PharmacyApp",
    "Audience": "PharmacyAppUsers",
    "ExpirationMinutes": 60
  }
}
```

### 3. Backend setup
#### Apply migrations
```bash
cd src/PharmacyApp.WebApi
dotnet ef database update --project ../PharmacyApp.Infrastructure
```

#### Run API
```bash
dotnet run --project src/PharmacyApp.WebApi
```

### 4. Frontend setup
```bash
cd frontend
npm install
npm run dev
```

### 5. Test admin login
```
"email": "admin@pharmacy.com",
"password": "Admin@123456"
```

### 6. IMPORTANT: Disable AdminBootstrap after first run
Set `"Enabled": false` in appsettings.

### Running the application
- API: `https://localhost:5001` (or current `launchSettings`)
- Frontend: default Next.js dev URL (usually `https://localhost:3000`)

## Railway Deployment (Backend)

### Create service
1. Create a new Railway service from this repository.
2. Set **Root Directory** to `src/PharmacyApp.WebApi`.
3. Add a PostgreSQL plugin (or shared Postgres service).

### Required variables
Set these variables in the backend Railway service:

```
ASPNETCORE_ENVIRONMENT=Production
JwtSettings__SecretKey=<at least 32 chars>
JwtSettings__Issuer=PharmacyApp
JwtSettings__Audience=PharmacyAppUsers
AdminBootstrap__Enabled=true
AdminBootstrap__Email=admin@yourdomain.com
AdminBootstrap__Password=<strong password>
AdminBootstrap__FirstName=System
AdminBootstrap__LastName=Administrator
Cors__AllowedOrigins__0=https://<your-frontend-domain>
Frontend__BaseUrl=https://<your-frontend-domain>
```

Database variable:
- Preferred: `ConnectionStrings__PharmacyAppConnection`
- Also supported now: Railway `DATABASE_URL` / `DATABASE_PRIVATE_URL` / `PG*` variables

### Important notes
- Migrations run automatically on API startup (`Database.MigrateAsync()`).
- Demo categories/products/content pages are seeded via EF migrations.
- If you connect to an old/non-empty DB and get duplicate key seed errors, create a fresh database and redeploy.
- After first successful deploy, set `AdminBootstrap__Enabled=false` to avoid accidental bootstrap changes.
