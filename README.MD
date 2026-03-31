# PharmacyApp

A modern Pharmacy Management System built with **Clean Architecture**, **ASP.NET Core 9.0** and **Entity Framework Core**.

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
- **Architecture**: Clean Architecture (Domain, Application, Infrastructure, Presentation, WebApi)
- **ORM**: Entity Framework Core (PostgreSQL support)
- **Containerization**: Docker, Docker Compose
- **Tools**: FluentValidation, MailKit

## Project Structure
- `PharmacyApp.Domain`: Core business entities (Product, Category, Review, etc.) and logic.
- `PharmacyApp.Application`: Application interfaces, DTOs, mapping, and business services.
- `PharmacyApp.Infrastructure`: Data access (EF Core), repository implementations and migrations.
-  `PharmacyApp.Presentation`: API Controllers and request/response handling
- `PharmacyApp.WebApi`: API Controllers and middleware.

## Getting Started

### 1. Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
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
    "LastName": "Administrator",
    "RequirePasswordReset": true
  }
}
```
####    - JWT secret key
```
{
  "JwtSettings": {
    "Secret": "your-super-secret-key-at-least-32-characters-long-for-production",
    "Issuer": "PharmacyApp",
    "Audience": "PharmacyAppUsers",
    "ExpirationMinutes": 60
  }
}
```

#### 3. Apply migrations
````
cd src/PharmacyApp.WebApi
dotnet ef database update --project ../PharmacyApp.Infrastructure
````

### 4. Run the application
````dotnet run````

### 5. Test admin login
```
"email": "admin@pharmacy.com",
"password": "Admin@123456"
```

### 6. IMPORTANT: Disable AdminBootstrap after first run
####    Set "Enabled": false in appsettings
### 3. Running the Application

By default, the API will be available at (https://localhost:5001) after starting the project.