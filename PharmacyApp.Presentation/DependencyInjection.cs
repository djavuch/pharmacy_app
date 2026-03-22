using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using PharmacyApp.Presentation.Filters;

namespace PharmacyApp.Presentation;
public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddScoped<FluentValidatorFilter>();
        
        services.Configure<MvcOptions>(options =>
        {
            options.Filters.AddService<FluentValidatorFilter>();
        });
        
        return services;
    }
}
