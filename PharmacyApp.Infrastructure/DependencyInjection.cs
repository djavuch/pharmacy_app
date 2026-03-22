using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using PharmacyApp.Application.Authorization.Requirements;
using PharmacyApp.Application.DTOs.Email;
using PharmacyApp.Application.Interfaces;
using PharmacyApp.Application.Interfaces.Email;
using PharmacyApp.Application.Interfaces.JWT;
using PharmacyApp.Application.Interfaces.RefreshTokens;
using PharmacyApp.Application.Interfaces.Repositories;
using PharmacyApp.Domain.Entities;
using PharmacyApp.Infrastructure.Data;
using PharmacyApp.Infrastructure.Repositories;
using PharmacyApp.Infrastructure.Services;
using System.Security.Claims;
using System.Text;
using PharmacyApp.Application.Interfaces.UserRoles;
using PharmacyApp.Infrastructure.Configuration.Admin;

namespace PharmacyApp.Infrastructure;
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<PharmacyAppDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("PharmacyAppConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string 'PharmacyAppConnection' not found.");
            }

            options.UseNpgsql(connectionString);
        });

        services.Configure<EmailConfigurationDto>(configuration.GetSection("EmailConfiguration"));
        services.Configure<AdminBootstrapOptions>(configuration.GetSection("AdminBootstrap"));

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IDiscountRepository, DiscountRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IShoppingCartRepository, ShoppingCartRepository>();
        services.AddScoped<IWishlistRepository, WishlistRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IAuthRepository, AuthRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IUnitOfWorkRepository, UnitOfWorkRepository>();
        services.AddScoped<IEmailSenderService, EmailSenderService>();
        services.AddScoped<IAccountNotificationSender, AccountNotificationSender>();
        services.AddScoped<IOrderEmailNotifier, OrderEmailNotifier>();
        services.AddScoped<IUserAddressRepository, UserAddressRepository>();
        services.AddScoped<IPromoCodeRepository, PromoCodeRepository>();
        services.AddScoped<IBonusRepository, BonusRepository>();
        services.AddScoped<IRoleInitializationService, RoleInitializationService>();
        

        services.AddSingleton<IBackgroundTaskQueue>(sp => new BackgroundTaskQueue(100));
        services.AddHostedService<QueueHostedService>();

        services.AddScoped<IJwtTokenProvider, JwtTokenProvider>();

        services.AddDataProtection()
           .SetApplicationName("PharmacyApp")
           .PersistKeysToFileSystem(new DirectoryInfo("/tmp/pharmacy-keys"));

        services.AddIdentity<UserModel, IdentityRole>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;

            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = true;

            options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultProvider;
            options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultProvider;
        })
            .AddEntityFrameworkStores<PharmacyAppDbContext>()
            .AddDefaultTokenProviders();

        services.Configure<DataProtectionTokenProviderOptions>(options =>
           options.TokenLifespan = TimeSpan.FromHours(2));

        // Jwt settings

        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"];

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["JwtSettings:Issuer"],
                    ValidAudience = configuration["JwtSettings:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    RoleClaimType = ClaimTypes.Role,    
                    NameClaimType = ClaimTypes.NameIdentifier
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("EmailConfirmed", policy =>
                policy.Requirements.Add(new EmailConfirmedRequirement()));
        });

        services.AddDistributedMemoryCache();
        services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });

        return services;
    }
}