namespace PharmacyApp.Application.Contracts.User.Results;

public record IdentityOperationResult
{
    public bool Succeeded { get; set; }
    public object? Data { get; set; }
    public IEnumerable<string> Errors { get; set; } = Enumerable.Empty<string>();
}
