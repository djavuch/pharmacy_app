using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using PharmacyApp.Presentation.Exceptions;
using PharmacyApp.Presentation.Filters;

namespace PharmacyApp.Presentation;
public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddScoped<FluentValidationFilter>();
        
        services.Configure<MvcOptions>(options =>
        {
            options.Filters.AddService<FluentValidationFilter>();
        });
        
        services.AddExceptionHandler<GlobalExceptionHandler>();
        
        return services;
    }
}