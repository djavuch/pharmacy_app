using System.Text.Json.Serialization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using PharmacyApp.Application;
using PharmacyApp.Infrastructure;
using PharmacyApp.Presentation;
using PharmacyApp.Application.Interfaces.UserRoles;
using PharmacyApp.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddOpenApi();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddPresentation();

// Collect allowed origins from config (supports both JSON array and
// environment variables using the double-underscore convention, e.g.
// Cors__AllowedOrigins__0=https://example.com).
// The production frontend origin is always included as a hard-coded
// fallback so CORS cannot be accidentally broken by a missing env var.
const string productionFrontend = "https://pharmacy-frontend-production-3729.up.railway.app";

var configuredOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>();

var allowedOrigins = (configuredOrigins is { Length: > 0 } ? configuredOrigins : ["https://localhost:3000"])
    .Union([productionFrontend])
    .Distinct()
    .ToArray();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseForwardedHeaders();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<PharmacyAppDbContext>();
    await dbContext.Database.MigrateAsync();
    
    var roleInitializationService = scope.ServiceProvider.GetRequiredService<IRoleInitializationService>();
    await roleInitializationService.InitializeAllAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

// CORS must run before HTTPS redirection so that preflight OPTIONS
// requests are not redirected (307) before the CORS headers are added.
app.UseCors("AllowFrontend");

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


app.Run();
