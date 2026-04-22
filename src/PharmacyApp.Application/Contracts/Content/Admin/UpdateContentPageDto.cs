namespace PharmacyApp.Application.Contracts.Content.Admin;

public record UpdateContentPageDto
{
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public bool IsPublished { get; init; } = true;
}
