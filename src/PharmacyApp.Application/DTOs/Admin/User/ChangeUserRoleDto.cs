namespace PharmacyApp.Application.DTOs.Admin.User;

public record ChangeUserRoleDto
{
    public string Role { get; init; } = string.Empty;
}