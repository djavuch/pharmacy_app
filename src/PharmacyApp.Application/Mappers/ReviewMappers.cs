using PharmacyApp.Application.Contracts.Review;
using PharmacyApp.Domain.Entities;

namespace PharmacyApp.Application.Mappers;

public static class ReviewMappers
{
    public static ProductReviewDto ToProductReviewDto(this Review review) => new()
    {
        Id = review.Id,
        ProductId = review.ProductId,
        ProductName = review.Product?.Name,
        FullName = $"{review.User?.FirstName} {review.User?.LastName}".Trim(),
        Rating = review.Rating,
        Content = review.Content,
        CreatedAt = review.CreatedAt
    };
}
