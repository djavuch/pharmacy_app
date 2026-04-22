namespace PharmacyApp.Domain.Entities;

public class ContentPage
{
    public int Id { get; private set; }
    public string Slug { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public bool IsPublished { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }

    private ContentPage()
    {
    }

    public ContentPage(
        string slug,
        string title,
        string content,
        bool isPublished,
        string? updatedBy)
    {
        Slug = slug;
        Title = title;
        Content = content;
        IsPublished = isPublished;
        UpdatedBy = updatedBy;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(string title, string content, bool isPublished, string? updatedBy)
    {
        Title = title;
        Content = content;
        IsPublished = isPublished;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }
}
