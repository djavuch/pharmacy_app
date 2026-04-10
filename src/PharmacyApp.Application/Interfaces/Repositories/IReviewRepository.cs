using PharmacyApp.Domain.Entities;

namespace PharmacyApp.Application.Interfaces.Repositories;
public interface IReviewRepository
{
    Task<Review?> GetByIdAsync(int id);
    IQueryable<Review> GetByProductId(int productId);
    Task AddAsync(Review review);
    // Admin specific
    public IQueryable<Review> GetAll();
    Task DeleteAsync(Review review);
}
