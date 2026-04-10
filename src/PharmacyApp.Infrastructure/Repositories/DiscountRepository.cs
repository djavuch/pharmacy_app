using Microsoft.EntityFrameworkCore;
using PharmacyApp.Application.Interfaces.Repositories;
using PharmacyApp.Domain.Entities.Discount;
using PharmacyApp.Infrastructure.Data;

namespace PharmacyApp.Infrastructure.Repositories;

public class DiscountRepository : IDiscountRepository
{
    private readonly PharmacyAppDbContext _dbContext;

    public DiscountRepository(PharmacyAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Discount?> GetByIdAsync(Guid discountId)
    {
        return await _dbContext.Discounts
            .Include(d => d.ProductDiscounts)
                .ThenInclude(pd => pd.Product)
            .Include(d => d.CategoryDiscounts)
                .ThenInclude(cd => cd.Category)
            .AsSplitQuery() 
            .FirstOrDefaultAsync(d => d.DiscountId == discountId);
    }

    public async Task<IEnumerable<Discount>> GetAllAsync()
    {
        return await _dbContext.Discounts
            .AsNoTracking()
            .Include(d => d.ProductDiscounts)
            .Include(d => d.CategoryDiscounts)
            .AsSplitQuery()
            .ToListAsync();
    }

    public async Task<IEnumerable<Discount>> GetActiveDiscountsAsync()
    {
        var currentDate = DateTime.UtcNow;
        return await _dbContext.Discounts
            .AsNoTracking()
            .Where(d => d.StartDate <= currentDate && d.EndDate >= currentDate)
            .Include(d => d.ProductDiscounts)
            .Include(d => d.CategoryDiscounts)
            .AsSplitQuery()
            .ToListAsync();
    }

    public async Task<Discount> AddAsync(Discount discount)
    {
        _dbContext.Discounts.Add(discount);
        await _dbContext.SaveChangesAsync();
        return discount;
    }

    public async Task UpdateAsync(Discount discount)
    {
        _dbContext.Discounts.Update(discount);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid discountId)
    {
        var discount = await _dbContext.Discounts.FindAsync(discountId);
        if (discount != null)
        {
            _dbContext.Discounts.Remove(discount);
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Discount>> GetDiscountsByProductIdAsync(int productId)
    {
        var now = DateTime.UtcNow;
        return await _dbContext.Discounts
            .Where(d => d.IsActive && d.StartDate <= now && d.EndDate >= now &&
                        d.ProductDiscounts.Any(pd => pd.ProductId == productId))
            .ToListAsync();
    }

    public async Task<IEnumerable<Discount>> GetDiscountsByCategoryIdAsync(int categoryId)
        {
            var now = DateTime.UtcNow;
            return await _dbContext.Discounts
                .Where(d => d.IsActive && d.StartDate <= now && d.EndDate >= now &&
                            d.CategoryDiscounts.Any(cd => cd.CategoryId == categoryId))
                .ToListAsync();
    }   
}