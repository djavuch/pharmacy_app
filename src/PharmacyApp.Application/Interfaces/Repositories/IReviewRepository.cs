using PharmacyApp.Application.Common;
using PharmacyApp.Domain.Entities;

namespace PharmacyApp.Application.Interfaces.Repositories;

public interface IReviewRepository
{
    Task<Review?> GetByIdAsync(int id);
    IQueryable<Review> GetByProductId(int productId);
    IQueryable<Review> GetApprovedByProductId(int productId);
    Task<ReviewStats> GetApprovedStatsAsync(int productId);
    Task<Dictionary<int, ReviewStats>> GetApprovedStatsByIdsAsync(IReadOnlyCollection<int> productIds);
    Task AddAsync(Review review);

    // Admin specific
    IQueryable<Review> GetAll();
    Task DeleteAsync(Review review);
}
