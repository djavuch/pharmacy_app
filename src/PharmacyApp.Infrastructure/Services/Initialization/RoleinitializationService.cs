using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PharmacyApp.Application.Interfaces.UserRoles;
using PharmacyApp.Domain.Entities;
using PharmacyApp.Infrastructure.Options;

namespace PharmacyApp.Infrastructure.Services.Initialization;

public class RoleInitializationService : IRoleInitializationService
{
    private static readonly string[] RequiredRoles = ["Admin", "Pharmacist", "Manager", "Customer"];

    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<UserModel> _userManager;
    private readonly IOptions<AdminBootstrapOptions> _options;
    private readonly ILogger<RoleInitializationService> _logger;

    public RoleInitializationService(
        RoleManager<IdentityRole> roleManager,
        UserManager<UserModel> userManager,
        IOptions<AdminBootstrapOptions> options,
        ILogger<RoleInitializationService> logger)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _options = options;
        _logger = logger;
    }

    public async Task InitializeRolesAsync()
    {
        foreach (var roleName in RequiredRoles)
        {
            if (await _roleManager.RoleExistsAsync(roleName))
            {
                continue;
            }

            var createRoleResult = await _roleManager.CreateAsync(new IdentityRole(roleName));
            if (!createRoleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to create role '{roleName}': {FormatIdentityErrors(createRoleResult.Errors)}");
            }

            _logger.LogInformation("Role '{RoleName}' created.", roleName);
        }
    }

    public async Task InitializeAdminUserAsync()
    {
        var options = _options.Value;

        if (!options.Enabled)
        {
            _logger.LogInformation("Admin bootstrap is disabled.");
            return;
        }

        if (string.IsNullOrWhiteSpace(options.Email) || string.IsNullOrWhiteSpace(options.Password))
        {
            throw new InvalidOperationException(
                "Admin bootstrap is enabled, but AdminBootstrap:Email or AdminBootstrap:Password is empty.");
        }

        var existingAdmins = await _userManager.GetUsersInRoleAsync("Admin");
        if (existingAdmins.Count > 0)
        {
            _logger.LogInformation("Admin user already exists. Bootstrap skipped.");
            return;
        }

        var user = await _userManager.FindByEmailAsync(options.Email);

        if (user is null)
        {
            user = new UserModel
            {
                UserName = options.Email,
                Email = options.Email,
                FirstName = options.FirstName,
                LastName = options.LastName,
                EmailConfirmed = true,
                DateOfBirth = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createUserResult = await _userManager.CreateAsync(user, options.Password);
            if (!createUserResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to create bootstrap admin user: {FormatIdentityErrors(createUserResult.Errors)}");
            }

            _logger.LogInformation("Bootstrap admin user created: {Email}", options.Email);
        }

        if (!await _userManager.IsInRoleAsync(user, "Admin"))
        {
            var addToRoleResult = await _userManager.AddToRoleAsync(user, "Admin");
            if (!addToRoleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to assign Admin role: {FormatIdentityErrors(addToRoleResult.Errors)}");
            }
        }

        if (options.RequirePasswordReset)
        {
            user.IsPasswordReset = true;
            user.UpdatedAt = DateTime.UtcNow;
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to set password reset flag for bootstrap admin: {FormatIdentityErrors(updateResult.Errors)}");
            }
        }

        _logger.LogInformation("Bootstrap admin is initialized.");
    }

    public async Task InitializeAllAsync()
    {
        await InitializeRolesAsync();
        await InitializeAdminUserAsync();
    }

    private static string FormatIdentityErrors(IEnumerable<IdentityError> errors)
        => string.Join("; ", errors.Select(e => $"{e.Code}: {e.Description}"));
}
