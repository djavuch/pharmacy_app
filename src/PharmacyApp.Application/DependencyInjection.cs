using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using PharmacyApp.Application.Authorization.Handlers;
using PharmacyApp.Application.DTOValidations.Orders;
using PharmacyApp.Application.Interfaces.Services;
using PharmacyApp.Application.Services;

namespace PharmacyApp.Application;
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<IShoppingCartService, ShoppingCartService>();
        services.AddScoped<IWishlistService, WishlistService>();
        services.AddScoped<IDiscountService, DiscountService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IUserAddressService, UserAddressService>();
        services.AddScoped<IPromoCodeService, PromoCodeService>();
        services.AddScoped<IBonusService, BonusService>();
        services.AddScoped<IAdminDashboardService, AdminDashboardService>();
        services.AddScoped<IContentPageService, ContentPageService>();

        // Authorization Handlers
        services.AddScoped<IAuthorizationHandler, EmailConfirmedHandler>();

        //Fluent Validation
        services.AddValidatorsFromAssemblyContaining<CreateOrderDtoValidator>();

        // Hybrid Cache Configuration
        services.AddHybridCache(options =>
        {
            options.MaximumPayloadBytes = 1024 * 1024; // 1 MB
            options.MaximumKeyLength = 512;
            options.DefaultEntryOptions = new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(10),
                LocalCacheExpiration = TimeSpan.FromMinutes(10),
            };
        });

        return services;
    }
}
