namespace PharmacyApp.Application.DTOs.Admin.User;

public record LockUserDto
{
    public DateTimeOffset? LockoutEnd { get; set; }
}