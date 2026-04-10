namespace PharmacyApp.Application.Contracts.Review;

public record ProductReviewDto
{
    public int Id { get; set; }
    public string? FullName { get; set; }
    public int ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? Content { get; set; }
    public int Rating { get; set; }
    public DateTime CreatedAt { get; set; }
}
