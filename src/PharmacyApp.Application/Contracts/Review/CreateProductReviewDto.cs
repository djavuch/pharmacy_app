namespace PharmacyApp.Application.Contracts.Review;

public record CreateProductReviewDto
{
    public int ProductId { get; set; }
    public int Rating { get; set; } // e.g., 1 to 5
    public string Content { get; set; } = string.Empty;
}
