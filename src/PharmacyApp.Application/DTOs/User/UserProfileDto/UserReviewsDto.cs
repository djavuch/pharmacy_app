namespace PharmacyApp.Application.DTOs.User.UserProfileDto;

public record UserReviewsDto
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
