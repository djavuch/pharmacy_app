namespace PharmacyApp.Application.Contracts.User.Account;

public record RefreshTokenRequestDto
{
    public string RefreshToken { get; set; } = string.Empty;
}
