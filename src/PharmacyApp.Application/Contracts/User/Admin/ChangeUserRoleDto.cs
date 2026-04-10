namespace PharmacyApp.Application.Contracts.User.Admin;

public record ChangeUserRoleDto
{
    public string Role { get; init; } = string.Empty;
}