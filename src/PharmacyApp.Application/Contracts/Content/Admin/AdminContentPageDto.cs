namespace PharmacyApp.Application.Contracts.Content.Admin;

public record AdminContentPageDto
{
    public int Id { get; init; }
    public string Slug { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public bool IsPublished { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public string? UpdatedBy { get; init; }
}
