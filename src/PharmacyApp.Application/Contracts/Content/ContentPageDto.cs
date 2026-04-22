namespace PharmacyApp.Application.Contracts.Content;

public record ContentPageDto
{
    public string Slug { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public DateTime UpdatedAt { get; init; }
}
