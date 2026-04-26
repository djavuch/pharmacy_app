using Microsoft.EntityFrameworkCore;
using PharmacyApp.Application.Common.Pagination;
using PharmacyApp.Application.Interfaces.Repositories;
using PharmacyApp.Domain.Entities.Bonus;
using PharmacyApp.Infrastructure.Data;

namespace PharmacyApp.Infrastructure.Repositories;

public class BonusRepository : IBonusRepository
{
    private readonly PharmacyAppDbContext _dbContext;

    public BonusRepository(PharmacyAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<BonusAccount?> GetByUserIdAsync(string userId)
    {
        return await _dbContext.BonusAccounts
            .FirstOrDefaultAsync(b => b.UserId == userId);
    }

    public async Task<IEnumerable<BonusAccount>> GetAllAccountsAsync(int pageIndex, int pageSize)
    {
        return await _dbContext.BonusAccounts
            .OrderByDescending(b => b.Balance)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public Task<BonusAccount> CreateAsync(BonusAccount account)
    {
        _dbContext.BonusAccounts.Add(account);
        return Task.FromResult(account);
    }

    public Task UpdateAsync(BonusAccount account)
    {
        _dbContext.BonusAccounts.Update(account);
        return Task.CompletedTask;
    }

    public Task AddTransactionAsync(BonusTransaction transaction)
    {
        _dbContext.BonusTransactions.Add(transaction);
        return Task.CompletedTask;
    }

    public async Task<PaginatedList<BonusTransaction>> GetTransactionsAsync(
        string userId,
        QueryParams queryParams)
    {
        var query = _dbContext.BonusTransactions
            .Where(t => t.BonusAccount.UserId == userId)
            .OrderByDescending(t => t.CreatedAt);

        return await PaginatedList<BonusTransaction>.CreateAsync(query, queryParams);
    }

    public async Task<IEnumerable<BonusTransaction>> GetTransactionsByOrderIdAsync(int orderId)
    {
        return await _dbContext.BonusTransactions
            .Where(t => t.OrderId == orderId)
            .ToListAsync();
    }

    public async Task<BonusSettings> GetSettingsAsync()
    {
        return await _dbContext.BonusSettings.FirstAsync(s => s.Id == 1);
    }

    public Task UpdateSettingsAsync(BonusSettings settings)
    {
        settings.UpdatedAt = DateTime.UtcNow;
        _dbContext.BonusSettings.Update(settings);
        return Task.CompletedTask;
    }
}
