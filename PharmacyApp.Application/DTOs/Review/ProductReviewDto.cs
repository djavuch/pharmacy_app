namespace PharmacyApp.Application.DTOs.Review;

public record ProductReviewDto
{
    public int Id { get; set; }
    public string? UserId { get; set; }
    public int ProductId { get; set; }
    public string? Content { get; set; }
    public int Rating { get; set; }
    public DateTime CreatedAt { get; set; }
}
