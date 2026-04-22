using Microsoft.EntityFrameworkCore;
using PharmacyApp.Application.Common;
using PharmacyApp.Application.Interfaces.Repositories;
using PharmacyApp.Domain.Entities;
using PharmacyApp.Domain.Enums;
using PharmacyApp.Infrastructure.Data;

namespace PharmacyApp.Infrastructure.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly PharmacyAppDbContext _dbContext;

    public ReviewRepository(PharmacyAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Review?> GetByIdAsync(int id)
    {
        return await _dbContext.Reviews
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public IQueryable<Review> GetByProductId(int productId)
    {
        return _dbContext.Reviews
            .AsNoTracking()
            .Where(r => r.ProductId == productId);
    }

    public IQueryable<Review> GetApprovedByProductId(int productId)
    {
        return _dbContext.Reviews
            .AsNoTracking()
            .Where(r => r.ProductId == productId && r.Status == ReviewStatus.Approved);
    }

    public async Task<ReviewStats> GetApprovedStatsAsync(int productId)
    {
        var stats = await _dbContext.Reviews
            .AsNoTracking()
            .Where(r => r.ProductId == productId && r.Status == ReviewStatus.Approved)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Count = g.Count(),
                AverageRating = g.Average(r => (decimal)r.Rating)
            })
            .FirstOrDefaultAsync();

        if (stats is null)
            return default;

        return new ReviewStats(stats.Count, decimal.Round(stats.AverageRating, 1));
    }

    public async Task<Dictionary<int, ReviewStats>> GetApprovedStatsByIdsAsync(IReadOnlyCollection<int> productIds)
    {
        if (productIds.Count == 0)
            return new Dictionary<int, ReviewStats>();

        var stats = await _dbContext.Reviews
            .AsNoTracking()
            .Where(r => productIds.Contains(r.ProductId) && r.Status == ReviewStatus.Approved)
            .GroupBy(r => r.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                Count = g.Count(),
                AverageRating = g.Average(r => (decimal)r.Rating)
            })
            .ToListAsync();

        return stats.ToDictionary(
            x => x.ProductId,
            x => new ReviewStats(x.Count, decimal.Round(x.AverageRating, 1)));
    }

    public async Task AddAsync(Review review)
    {
        await _dbContext.Reviews.AddAsync(review);
    }

    // Admin specific
    public IQueryable<Review> GetAll()
    {
        return _dbContext.Reviews.AsQueryable();
    }

    public async Task DeleteAsync(Review review)
    {
        var reviewToDelete = await _dbContext.Reviews.FindAsync(review.Id);

        if (reviewToDelete != null)
        {
            _dbContext.Reviews.Remove(reviewToDelete);
        }
    }
}
