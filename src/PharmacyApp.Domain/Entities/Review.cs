using PharmacyApp.Domain.Enums;

namespace PharmacyApp.Domain.Entities;

public class Review
{
    public int Id { get; private set; }
    public string UserId { get; private set; }
    public int ProductId { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public int Rating { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.Now.ToUniversalTime();
    public ReviewStatus Status { get; private set; } = ReviewStatus.Pending;
    public Product? Product { get; set; }
    public User? User { get; set; }

    private Review() { }

    public Review(string userId, int productId, int rating, string? content)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("UserId cannot be empty.");
        if (productId <= 0)
            throw new ArgumentException("ProductId must be greater than 0.");
        if (rating < 1 || rating > 5)
            throw new ArgumentException("Rating must be between 1 and 5.");

        UserId = userId;
        ProductId = productId;
        Rating = rating;
        Content = content ?? string.Empty;
        CreatedAt = DateTime.UtcNow;
        Status = ReviewStatus.Pending;
    }

    public void Approve()
    {
        Status = ReviewStatus.Approved;
    }

    public void Reject()
    {
        Status = ReviewStatus.Rejected;
    }
}
