namespace PharmacyApp.Application.DTOs.User.AccountDto;

public record IdentityResultDto
{
    public bool Succeeded { get; set; }
    public object? Data { get; set; }
    public IEnumerable<string> Errors { get; set; } = Enumerable.Empty<string>();
}
