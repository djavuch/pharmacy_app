using System.Text.Json.Serialization;
using PharmacyApp.Application;
using PharmacyApp.Infrastructure;
using PharmacyApp.Presentation;
using PharmacyApp.Application.Interfaces.UserRoles;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddPresentation();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

builder.Services.AddProblemDetails();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var roleInitializationService = scope.ServiceProvider.GetRequiredService<IRoleInitializationService>();
    await roleInitializationService.InitializeAllAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();