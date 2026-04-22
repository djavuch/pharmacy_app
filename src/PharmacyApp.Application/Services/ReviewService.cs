using Microsoft.Extensions.Caching.Hybrid;
using PharmacyApp.Application.Common;
using PharmacyApp.Application.Common.Pagination;
using PharmacyApp.Application.Contracts.Review;
using PharmacyApp.Application.Contracts.Review.Admin;
using PharmacyApp.Application.Interfaces;
using PharmacyApp.Application.Interfaces.Repositories;
using PharmacyApp.Application.Interfaces.Services;
using PharmacyApp.Application.Mappers;
using PharmacyApp.Domain.Common;
using PharmacyApp.Domain.Entities;
using PharmacyApp.Domain.Enums;

namespace PharmacyApp.Application.Services;

public class ReviewService : IReviewService
{
    private readonly IUnitOfWorkRepository _unitOfWork;
    private readonly HybridCache _cache;

    public ReviewService(IUnitOfWorkRepository unitOfWork,  HybridCache cache)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<Result<ProductReviewDto>> GetByIdAsync(int id)
    {
        var review = await _unitOfWork.Reviews.GetByIdAsync(id);

        if (review is null)
            return Result<ProductReviewDto>.NotFound("Review not found.");

        return Result<ProductReviewDto>.Success(review.ToProductReviewDto());
    }

    public async Task<PaginatedList<ProductReviewDto>> GetReviewsByProductIdAsync(int productId, QueryParams query)
    {
        return await _cache.GetOrCreateAsync(CacheKeys.Reviews.ByProduct(productId, query), async _ =>
        {
            var reviewsQuery = _unitOfWork.Reviews
                .GetApprovedByProductId(productId);

            return await PaginatedList<ProductReviewDto>.CreateAsync(
                reviewsQuery.Select(r => new ProductReviewDto
                {
                    Id = r.Id,
                    ProductId = r.ProductId,
                    ProductName = r.Product != null ? r.Product.Name : null,
                    FullName = (r.User != null ? r.User.FirstName : "") + " " + (r.User != null ? r.User.LastName : ""),
                    Rating = r.Rating,
                    Content = r.Content,
                    CreatedAt = r.CreatedAt
                }),
                query.PageIndex,
                query.PageSize);
        }, new HybridCacheEntryOptions { Expiration = TimeSpan.FromMinutes(10) });
    }

    public async Task<Result<ProductReviewDto>> AddReviewAsync(CreateProductReviewDto reviewDto, string userId)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(reviewDto.ProductId);

        if (product is null)
            return Result<ProductReviewDto>.NotFound("Product not found.");
        
        var review = new Review(userId, reviewDto.ProductId, reviewDto.Rating, reviewDto.Content.Trim());

        await _unitOfWork.Reviews.AddAsync(review); 
        await _unitOfWork.SaveChangesAsync();

        return Result<ProductReviewDto>.Success(review.ToProductReviewDto());
    }

    // Admin specific methods
    public async Task<PaginatedList<AdminReviewDto>> GetAllReviewsAsync(ReviewQueryParams queryParams)
    {
        var reviewsQuery = _unitOfWork.Reviews.GetAll();

        if (!string.IsNullOrWhiteSpace(queryParams.FilterOn) && !string.IsNullOrWhiteSpace(queryParams.FilterQuery))
        {
            reviewsQuery = queryParams.FilterOn?.ToLowerInvariant() switch
            {
                "product" => reviewsQuery.Where(r => r.Product != null && r.Product.Name.Contains(queryParams.FilterQuery)),
                "user" => reviewsQuery.Where(r => r.User != null && r.User.UserName != null && r.User.UserName.Contains(queryParams.FilterQuery)),
                _ => reviewsQuery,
                
            };
        }
        if (queryParams.Status.HasValue)
        {
            reviewsQuery = reviewsQuery.Where(r => r.Status == queryParams.Status.Value);
        }

        reviewsQuery = queryParams.SortBy?.ToLowerInvariant() switch
        {
            "rating" => queryParams.IsAscending ? reviewsQuery.OrderBy(r => r.Rating) : reviewsQuery.OrderByDescending(r => r.Rating),
            "createdat" => queryParams.IsAscending ? reviewsQuery.OrderBy(r => r.CreatedAt) : reviewsQuery.OrderByDescending(r => r.CreatedAt),
            "product" => queryParams.IsAscending ? reviewsQuery.OrderBy(r => r.Product!.Name) : reviewsQuery.OrderByDescending(r => r.Product!.Name),
            "user" => queryParams.IsAscending ? reviewsQuery.OrderBy(r => r.User!.UserName) : reviewsQuery.OrderByDescending(r => r.User!.UserName),
            _ => reviewsQuery.OrderByDescending(r => r.CreatedAt) 
        };

        return await PaginatedList<AdminReviewDto>.CreateAsync(
            reviewsQuery.Select(r => new AdminReviewDto
            {
                Id = r.Id,
                ProductId = r.ProductId,
                ProductName = r.Product != null ? r.Product.Name : null,
                UserId = r.UserId,
                UserName = r.User != null ? r.User.UserName : null,
                Rating = r.Rating,
                Content = r.Content,
                Status = r.Status,
                CreatedAt = r.CreatedAt
            }),
            queryParams.PageIndex, queryParams.PageSize);
    }

    public async Task<bool> ApproveReviewAsync(int reviewId)
    {
        var review = await _unitOfWork.Reviews.GetByIdAsync(reviewId);

        if (review is null)
        {
            return false;
        }

        review.Approve();
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RejectReviewAsync(int reviewId)
    {
        var review = await _unitOfWork.Reviews.GetByIdAsync(reviewId);

        if (review is null)
        {
            return false;
        }

        review.Reject();
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}
