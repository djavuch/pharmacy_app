using PharmacyApp.Domain.Enums;

namespace PharmacyApp.Application.Contracts.Review.Admin;

public record AdminReviewDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public int Rating { get; set; }
    public string? Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public ReviewStatus Status { get; set; }
}
