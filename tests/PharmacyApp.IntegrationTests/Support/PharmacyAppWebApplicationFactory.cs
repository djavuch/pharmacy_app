using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using PharmacyApp.Application.Interfaces.Abstractions;
using PharmacyApp.Application.Interfaces.Email;
using PharmacyApp.Domain.Entities;
using PharmacyApp.Domain.Entities.PromoCode;
using PharmacyApp.Domain.Enums;
using PharmacyApp.Infrastructure.Data;

namespace PharmacyApp.IntegrationTests.Support;

public sealed class PharmacyAppWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    private readonly Dictionary<string, string?> _previousEnvironmentVariables = new();

    public PharmacyAppWebApplicationFactory()
    {
        SetEnvironmentVariable("ConnectionStrings__PharmacyAppConnection", "Host=localhost;Database=pharmacy_tests;Username=test;Password=test");
        SetEnvironmentVariable("JwtSettings__SecretKey", "integration-test-secret-key-with-more-than-32-chars");
        SetEnvironmentVariable("JwtSettings__Issuer", "PharmacyApp.Tests");
        SetEnvironmentVariable("JwtSettings__Audience", "PharmacyApp.Tests");
        SetEnvironmentVariable("JwtSettings__ExpirationMinutes", "60");
        SetEnvironmentVariable("JwtSettings__RefreshTokenExpirationDays", "7");
        SetEnvironmentVariable("Frontend__BaseUrl", "https://localhost:3000");
        SetEnvironmentVariable("AdminBootstrap__Enabled", "false");

        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:PharmacyAppConnection"] = "Host=localhost;Database=pharmacy_tests;Username=test;Password=test",
                ["JwtSettings:SecretKey"] = "integration-test-secret-key-with-more-than-32-chars",
                ["JwtSettings:Issuer"] = "PharmacyApp.Tests",
                ["JwtSettings:Audience"] = "PharmacyApp.Tests",
                ["JwtSettings:ExpirationMinutes"] = "60",
                ["JwtSettings:RefreshTokenExpirationDays"] = "7",
                ["Frontend:BaseUrl"] = "https://localhost:3000",
                ["AdminBootstrap:Enabled"] = "false"
            });
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<PharmacyAppDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<PharmacyAppDbContext>>();
            services.RemoveAll<IHostedService>();
            services.RemoveAll<IBackgroundTaskQueue>();
            services.RemoveAll<IOrderEmailNotifier>();

            services.AddDbContext<PharmacyAppDbContext>(options => options.UseSqlite(_connection));
            services.AddSingleton<IBackgroundTaskQueue, NoOpBackgroundTaskQueue>();
            services.AddSingleton<IOrderEmailNotifier, NoOpOrderEmailNotifier>();
        });
    }

    private void SetEnvironmentVariable(string key, string value)
    {
        _previousEnvironmentVariables.TryAdd(key, Environment.GetEnvironmentVariable(key));
        Environment.SetEnvironmentVariable(key, value);
    }

    public async Task<User> CreateConfirmedUserAsync(
        string email = "customer@example.test",
        string password = "Password123")
    {
        using var scope = Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

        var user = new User
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FirstName = "Test",
            LastName = "Customer",
            DateOfBirth = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            CreatedAt = DateTime.UtcNow
        };

        var createResult = await userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
            throw new InvalidOperationException(string.Join("; ", createResult.Errors.Select(error => error.Description)));

        var roleResult = await userManager.AddToRoleAsync(user, "Customer");
        if (!roleResult.Succeeded)
            throw new InvalidOperationException(string.Join("; ", roleResult.Errors.Select(error => error.Description)));

        return user;
    }

    public async Task<int> CreateProductAsync(
        string name = "Integration Test Product",
        decimal price = 25m,
        int stockQuantity = 10)
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PharmacyAppDbContext>();

        var category = new Category("Integration Test Category", "Created by integration tests.");
        var product = new Product(name, "Created by integration tests.", price, stockQuantity, "/images/test-product.png", category);

        dbContext.Categories.Add(category);
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        return product.Id;
    }

    public async Task<Guid> CreatePromoCodeAsync(
        string code = "SAVE10",
        decimal value = 10m,
        DiscountType discountType = DiscountType.FixedAmount)
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PharmacyAppDbContext>();

        var promoCode = new PromoCode(
            code,
            "Created by integration tests.",
            discountType,
            value,
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow.AddDays(1),
            applicableToAllProducts: true);

        dbContext.PromoCodes.Add(promoCode);
        await dbContext.SaveChangesAsync();

        return promoCode.PromoCodeId;
    }

    public async Task<T> ExecuteDbContextAsync<T>(Func<PharmacyAppDbContext, Task<T>> action)
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PharmacyAppDbContext>();
        return await action(dbContext);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _connection.Dispose();

            foreach (var (key, previousValue) in _previousEnvironmentVariables)
                Environment.SetEnvironmentVariable(key, previousValue);
        }
    }
}
