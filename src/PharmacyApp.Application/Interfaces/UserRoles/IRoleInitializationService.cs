namespace PharmacyApp.Application.Interfaces.UserRoles;

public interface IRoleInitializationService
{
    Task InitializeRolesAsync();
    Task InitializeAdminUserAsync();
    Task InitializeAllAsync();
}