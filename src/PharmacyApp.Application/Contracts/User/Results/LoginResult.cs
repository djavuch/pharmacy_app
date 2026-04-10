using PharmacyApp.Application.Contracts.User.Results;

namespace PharmacyApp.Application.Contracts.User.Results;

public record LoginResult
{
    public bool Succeeded { get; set; } 
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
    public string? UserId { get; set; }
    public LoginFailureReason? FailureReason { get; set; }
    public string? Message { get; set; }
}
