using PharmacyApp.Application.DTOs.Admin.Review;
using PharmacyApp.Application.DTOs.Common;
using PharmacyApp.Application.DTOs.Review;

namespace PharmacyApp.Application.Interfaces.Services;

public interface IReviewService
{
    Task<ProductReviewDto> GetByIdAsync(int id);
    Task<PaginatedList<ProductReviewDto>> GetReviewsByProductIdAsync(int productId, int pageIndex, int pageSize);
    Task<ProductReviewDto> AddReviewAsync(CreateProductReviewDto reviewDto, string userId);

    // Admin specific methods
    Task<PaginatedList<AdminReviewDto>> GetAllReviewsAsync(int pageIndex = 1, int pageSize = 10,string? filterOn = null,
        string? filterQuery = null, bool? isApproved = null, string? sortBy = null, bool isAscending = true);
    Task<bool> ApproveReviewAsync(int reviewId);
    Task<bool> RejectReviewAsync(int reviewId);
}