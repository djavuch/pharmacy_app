# PharmacyApp

A modern Pharmacy Management System built with **Clean Architecture**, **ASP.NET Core 9.0** and **Entity Framework Core**.

## Project Overview
PharmacyApp is a robust RESTful API designed to handle pharmaceutical products, categories, shopping carts, bonus loyalty systems and customer reviews.

### Key Features
- **Product Management**: A complete catalog of medications with category filtering and dynamic discount handling.
- **Bonus System**: Automated accrual and deduction of bonus points during purchases.
- **Cart & Orders**: Full lifecycle of order processing from cart management to checkout.
- **Reviews & Ratings**: Customer feedback system for products.
- **Demo Seeds**: Includes initial seed data for categories and products to demonstrate system capabilities.
- **Security**: Secure authentication and authorization (Identity/JWT).
- **First User Admin**: The very first registered user is automatically granted access to admin functions.
- **Concurrency**: Race condition handling using `RowVersion` timestamps for data integrity.
- **Containerization**: Ready for deployment with Docker and Docker Compose.

## Technology Stack
- **Backend**: C# 13, .NET 9, ASP.NET Core Web API
- **Architecture**: Clean Architecture (Domain, Application, Infrastructure, WebApi)
- **Database**: Entity Framework Core (PostgreSQL support)
- **Containerization**: Docker, Docker Compose
- **Tools**: FluentValidation, MailKit

## Project Structure
- `PharmacyApp.Domain`: Core business entities (Product, Category, Review, etc.) and logic.
- `PharmacyApp.Application`: Application interfaces, DTOs, mapping, and business services.
- `PharmacyApp.Infrastructure`: Data access (EF Core), repository implementations and migrations.
- `PharmacyApp.WebApi`: API Controllers and middleware.

## Getting Started

### 1. Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (optional)

### 2. Installation
```bash
git clone https://github.com/djavuch/PharmacyApp.git
cd PharmacyApp
```
### 3. Running the Application

By default, the API will be available at (https://localhost:5001) after starting the project.