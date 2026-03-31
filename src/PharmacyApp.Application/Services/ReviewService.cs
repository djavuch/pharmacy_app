using Microsoft.EntityFrameworkCore;
using PharmacyApp.Application.DTOs.Admin.Review;
using PharmacyApp.Application.DTOs.Common;
using PharmacyApp.Application.DTOs.Review;
using PharmacyApp.Application.Interfaces;
using PharmacyApp.Application.Interfaces.Services;
using PharmacyApp.Application.Mappers;
using PharmacyApp.Domain.Entities;
using static PharmacyApp.Domain.Exceptions.AppExceptions;

namespace PharmacyApp.Application.Services;

public class ReviewService : IReviewService
{
    private readonly IUnitOfWorkRepository _unitOfWork;

    public ReviewService(IUnitOfWorkRepository unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ProductReviewDto> GetByIdAsync(int id)
    {
        var review = await _unitOfWork.Reviews.GetByIdAsync(id); 

        if (review is null)
        {
            throw new NotFoundException("Review not found.");
        }

        return review.ToProductReviewDto();
    }

    public async Task<PaginatedList<ProductReviewDto>> GetReviewsByProductIdAsync(int productId, int pageIndex = 1, int pageSize = 10)
    {
        var query = _unitOfWork.Reviews
            .GetByProductId(productId)
            .Where(r => r.IsApproved);

        var totalReviews = await query.CountAsync();

        var pagedReviews = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new ProductReviewDto 
            {
                Id = r.Id,
                ProductId = r.ProductId,
                ProductName = r.Product != null ? r.Product.Name : null,
                FullName = (r.User != null ? r.User.FirstName : "") + " " + (r.User != null ? r.User.LastName : ""),
                Rating = r.Rating,
                Content = r.Content,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        return new PaginatedList<ProductReviewDto>
        {
            Items = pagedReviews,
            PageIndex = pageIndex,
            TotalPages = (int)Math.Ceiling(totalReviews / (double)pageSize),
            PageSize = pageSize
        };
    }

    public async Task<ProductReviewDto> AddReviewAsync(CreateProductReviewDto reviewDto, string userId)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(reviewDto.ProductId);

        if (product is null)
        {
            throw new NotFoundException("Product not found.");
        }

        var review = new ReviewModel
        {
            ProductId = reviewDto.ProductId,
            UserId = userId,
            Rating = reviewDto.Rating,
            Content = reviewDto.Content,
            CreatedAt = DateTime.UtcNow,
            IsApproved = false // Reviews need to be approved before being visible
        };

        await _unitOfWork.Reviews.AddAsync(review); 
        await _unitOfWork.SaveChangesAsync();

        return review.ToProductReviewDto();
    }

    // Admin specific methods
    public async Task<PaginatedList<AdminReviewDto>> GetAllReviewsAsync(
        int pageIndex = 1,
        int pageSize = 10,
        string? filterOn = null,
        string? filterQuery = null,
        bool? isApproved = null,
        string? sortBy = null,
        bool isAscending = true)
    {
        var reviewsQuery = _unitOfWork.Reviews.GetAll();

        if (!string.IsNullOrWhiteSpace(filterOn) && !string.IsNullOrWhiteSpace(filterQuery))
        {
            reviewsQuery = filterOn?.ToLowerInvariant() switch
            {
                "product" => reviewsQuery.Where(r => r.Product != null && r.Product.Name.Contains(filterQuery)),
                "user" => reviewsQuery.Where(r => r.User != null && r.User.UserName != null && r.User.UserName.Contains(filterQuery)),
                _ => reviewsQuery,
                
            };
        }
        if (isApproved.HasValue)
        {
            reviewsQuery = reviewsQuery.Where(r => r.IsApproved == isApproved.Value);
        }

        reviewsQuery = sortBy?.ToLowerInvariant() switch
        {
            "rating" => isAscending ? reviewsQuery.OrderBy(r => r.Rating) : reviewsQuery.OrderByDescending(r => r.Rating),
            "createdat" => isAscending ? reviewsQuery.OrderBy(r => r.CreatedAt) : reviewsQuery.OrderByDescending(r => r.CreatedAt),
            "product" => isAscending ? reviewsQuery.OrderBy(r => r.Product!.Name) : reviewsQuery.OrderByDescending(r => r.Product!.Name),
            "user" => isAscending ? reviewsQuery.OrderBy(r => r.User!.UserName) : reviewsQuery.OrderByDescending(r => r.User!.UserName),
            _ => reviewsQuery.OrderByDescending(r => r.CreatedAt) 
        };

        var totalCount = await reviewsQuery.CountAsync();
        var skipResults = (pageIndex - 1) * pageSize;
        var items = await reviewsQuery
            .Skip(skipResults)
            .Take(pageSize)
            .Select(r => new AdminReviewDto
            {
                Id = r.Id,
                ProductId = r.ProductId,
                ProductName = r.Product != null ? r.Product.Name : null,
                UserId = r.UserId,
                UserName = r.User != null ? r.User.UserName : null,
                Rating = r.Rating,
                Content = r.Content,
                IsApproved = r.IsApproved,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();


        return new PaginatedList<AdminReviewDto>
        {
            Items = items,
            PageIndex = pageIndex,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            PageSize = pageSize
        };
    }

    public async Task<bool> ApproveReviewAsync(int reviewId)
    {
        var review = await _unitOfWork.Reviews.GetByIdAsync(reviewId);

        if (review is null)
        {
            return false;
        }

        review.IsApproved = true;
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
        review.IsApproved = false;
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public void DeleteReview(ReviewModel review)
    {
        _unitOfWork.Reviews.DeleteAsync(review);
    }
}
