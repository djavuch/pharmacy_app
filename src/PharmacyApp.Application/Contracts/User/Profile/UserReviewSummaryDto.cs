namespace PharmacyApp.Application.Contracts.User.Profile;

public record UserReviewSummaryDto
{
    public int ReviewId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public int Rating { get; set; }
    public string Content { get; set; }
    public DateTime ReviewDate { get; set; }
    public decimal TotalReviews { get; set; }
    public bool IsApproved { get; set; }
}