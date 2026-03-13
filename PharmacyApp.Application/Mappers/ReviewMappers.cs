using PharmacyApp.Application.DTOs.Admin.Review;
using PharmacyApp.Application.DTOs.Review;
using PharmacyApp.Domain.Entities;

namespace PharmacyApp.Application.Mappers;

public static class ReviewMappers
{
    public static ProductReviewDto ToProductReviewDto(this ReviewModel review) => new()
    {
        Id = review.Id,
        ProductId = review.ProductId,
        UserId = review.UserId,
        Rating = review.Rating,
        Content = review.Content,
        CreatedAt = review.CreatedAt
    };

    public static AdminReviewDto ToAdminReviewDto(this ReviewModel review) => new()
    {
        Id = review.Id,
        ProductId = review.ProductId,
        UserId = review.UserId,
        Rating = review.Rating,
        Content = review.Content,
        IsApproved = review.IsApproved,
        CreatedAt = review.CreatedAt
    };
}
