namespace PharmacyApp.Application.Contracts.User.Admin;

public record LockUserDto
{
    public DateTimeOffset? LockoutEnd { get; set; }
}