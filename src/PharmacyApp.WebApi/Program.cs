using System.Text.Json.Serialization;
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

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

builder.Services.AddProblemDetails();

var app = builder.Build();

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
    app.UseHttpsRedirection();
}

app.UseExceptionHandler();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


app.Run();
