using PharmacyApp.Domain.Entities;

namespace PharmacyApp.Application.Interfaces.Repositories;
public interface IReviewRepository
{
    Task<ReviewModel?> GetByIdAsync(int id);
    Task<IEnumerable<ReviewModel>> GetByProductIdAsync(int productId);
    Task AddAsync(ReviewModel review);
    // Admin specific
    public IQueryable<ReviewModel> GetAll();
    Task DeleteAsync(ReviewModel review);
}
