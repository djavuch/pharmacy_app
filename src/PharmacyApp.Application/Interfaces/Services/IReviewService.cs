using PharmacyApp.Application.Common.Pagination;
using PharmacyApp.Application.Contracts.Review;
using PharmacyApp.Application.Contracts.Review.Admin;
using PharmacyApp.Domain.Common;

namespace PharmacyApp.Application.Interfaces.Services;

public interface IReviewService
{
    Task<Result<ProductReviewDto>> GetByIdAsync(int id);
    Task<PaginatedList<ProductReviewDto>> GetReviewsByProductIdAsync(int productId, QueryParams query);
    Task<Result<ProductReviewDto>> AddReviewAsync(CreateProductReviewDto reviewDto, string userId);

    // Admin specific methods
    Task<PaginatedList<AdminReviewDto>> GetAllReviewsAsync(ReviewQueryParams queryParams);
    Task<bool> ApproveReviewAsync(int reviewId);
    Task<bool> RejectReviewAsync(int reviewId);
}