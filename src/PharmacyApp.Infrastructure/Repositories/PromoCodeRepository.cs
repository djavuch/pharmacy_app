using Microsoft.EntityFrameworkCore;
using PharmacyApp.Application.Interfaces.Repositories;
using PharmacyApp.Domain.Entities.PromoCode;
using PharmacyApp.Infrastructure.Data;

namespace PharmacyApp.Infrastructure.Repositories;

public class PromoCodeRepository : IPromoCodeRepository
{
    private readonly PharmacyAppDbContext _dbContext;

    public PromoCodeRepository(PharmacyAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PromoCode?> GetByIdAsync(Guid promoCodeId)
    {
        return await _dbContext.PromoCodes
            .Include(p => p.PromoCodeProducts)
            .Include(p => p.PromoCodeCategories)
            .Include(p => p.UsageHistory)
            .FirstOrDefaultAsync(p => p.PromoCodeId == promoCodeId);
    }

    public async Task<PromoCode?> GetByCodeAsync(string code)
    {
        return await _dbContext.PromoCodes
            .Include(p => p.PromoCodeProducts)
            .Include(p => p.PromoCodeCategories)
            .Include(p => p.UsageHistory)
            .FirstOrDefaultAsync(p => p.Code == code.ToUpper());
    }

    public async Task<IEnumerable<PromoCode>> GetAllAsync()
    {
        return await _dbContext.PromoCodes
            .Include(p => p.PromoCodeProducts)
            .Include(p => p.PromoCodeCategories)
            .ToListAsync();
    }

    public async Task<IEnumerable<PromoCode>> GetActivePromoCodesAsync()
    {
        var now = DateTime.UtcNow;
        return await _dbContext.PromoCodes
            .Include(p => p.PromoCodeProducts)
            .Include(p => p.PromoCodeCategories)
            .Where(p => p.IsActive && p.StartDate <= now && p.EndDate >= now)
            .ToListAsync();
    }

    public async Task<PromoCode> AddAsync(PromoCode promoCode)
    {
        await _dbContext.PromoCodes.AddAsync(promoCode);
        return promoCode;
    }

    public Task UpdateAsync(PromoCode promoCode)
    {
        _dbContext.PromoCodes.Update(promoCode);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid promoCodeId)
    {
        var promoCode = await _dbContext.PromoCodes.FindAsync(promoCodeId);
        if (promoCode is not null)
        {
            _dbContext.PromoCodes.Remove(promoCode);
        }
    }

    public async Task<bool> CodeExistsAsync(string code, Guid? excludePromoCodeId = null)
    {
        var query = _dbContext.PromoCodes.Where(p => p.Code == code.ToUpper());

        if (excludePromoCodeId.HasValue)
            query = query.Where(p => p.PromoCodeId != excludePromoCodeId.Value);

        return await query.AnyAsync();
    }

    public async Task<int> GetUserUsageCountAsync(Guid promoCodeId, string userId)
    {
        return await _dbContext.PromoCodeUsages
            .CountAsync(u => u.PromoCodeId == promoCodeId && u.UserId == userId);
    }

    public Task<int> IncrementUsageAsync(Guid promoCodeId)
    {
        return _dbContext.PromoCodes
            .Where(p => p.PromoCodeId == promoCodeId)
            .ExecuteUpdateAsync(s => s.SetProperty(
                p => p.CurrentUsageCount, 
                p => p.CurrentUsageCount + 1));
    }
    
    public Task<int> DecrementUsageAsync(Guid promoCodeId)
    {
        return _dbContext.PromoCodes
            .Where(p => p.PromoCodeId == promoCodeId)
            .ExecuteUpdateAsync(s => s.SetProperty(
                p => p.CurrentUsageCount, 
                p => p.CurrentUsageCount > 0 ? p.CurrentUsageCount - 1 : 0));
    }

    public Task<int> RemoveUsageByOrderIdAsync(int orderId)
    {
        return _dbContext.PromoCodeUsages
            .Where(u => u.OrderId == orderId)
            .ExecuteDeleteAsync();
    }

    public async Task RecordUsageAsync(PromoCodeUsage usage)
    {
        await _dbContext.PromoCodeUsages.AddAsync(usage);
    }
}