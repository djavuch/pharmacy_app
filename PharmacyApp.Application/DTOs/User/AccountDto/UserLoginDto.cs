namespace PharmacyApp.Application.DTOs.User.AccountDto;

public record UserLoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; } = false;
}
