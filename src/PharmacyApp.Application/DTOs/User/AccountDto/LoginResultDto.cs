using PharmacyApp.Application.DTOs.User.Enums;

namespace PharmacyApp.Application.DTOs.User.AccountDto;

public record LoginResultDto
{
    public bool Succeeded { get; set; } 
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
    public string? UserId { get; set; }
    public LoginFailureReason? FailureReason { get; set; }
    public string? Message { get; set; }
}
