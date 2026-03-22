namespace PharmacyApp.Application.DTOs.User.UserProfileDto;

public record UpdateUserDto
{
    public string UserId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
}
