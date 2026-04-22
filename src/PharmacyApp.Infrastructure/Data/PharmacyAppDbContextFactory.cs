using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace PharmacyApp.Infrastructure.Data;

public class PharmacyAppDbContextFactory : IDesignTimeDbContextFactory<PharmacyAppDbContext>
{
    public PharmacyAppDbContext CreateDbContext(string[] args)
    {
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "PharmacyApp.WebApi");

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("PharmacyAppConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'PharmacyAppConnection' not found.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<PharmacyAppDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new PharmacyAppDbContext(optionsBuilder.Options);
    }
}
