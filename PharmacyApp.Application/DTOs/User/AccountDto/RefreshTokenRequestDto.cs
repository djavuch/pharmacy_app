namespace PharmacyApp.Application.DTOs.User.AccountDto;

public record RefreshTokenRequestDto
{
    public string RefreshToken { get; set; } = string.Empty;
}
