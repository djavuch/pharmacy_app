using Microsoft.EntityFrameworkCore;
using PharmacyApp.Application.Interfaces.Repositories;
using PharmacyApp.Domain.Entities;
using PharmacyApp.Infrastructure.Data;

namespace PharmacyApp.Infrastructure.Repositories;
public class ReviewRepository : IReviewRepository
{
    private readonly PharmacyAppDbContext _dbContext;

    public ReviewRepository(PharmacyAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ReviewModel?> GetByIdAsync(int id)
    {
        return await _dbContext.Reviews
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public IQueryable<ReviewModel> GetByProductId(int productId)
    {
        return _dbContext.Reviews
            .AsNoTracking()
            .Where(r => r.ProductId == productId);
    }

    public async Task AddAsync(ReviewModel review)
    {
        await _dbContext.Reviews.AddAsync(review);
    }

    //Admin specific
    public IQueryable<ReviewModel> GetAll()
    {
        return _dbContext.Reviews.AsQueryable();
    }

    public async Task DeleteAsync(ReviewModel review)
    {
        var reviewToDelete = await _dbContext.Reviews.FindAsync(review.Id);

        if (reviewToDelete != null)
        {
            _dbContext.Reviews.Remove(reviewToDelete);
        }
    }
}
