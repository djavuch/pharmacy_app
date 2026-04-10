namespace PharmacyApp.Application.Contracts.User.Account;

public record ForgotPasswordDto
{
    public string Email { get; set; } = string.Empty;
}
