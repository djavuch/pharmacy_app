using Microsoft.EntityFrameworkCore;
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

    public async Task<BonusAccountModel?> GetByUserIdAsync(string userId)
    {
        return await _dbContext.BonusAccounts
            .FirstOrDefaultAsync(b => b.UserId == userId);
    }

    public async Task<IEnumerable<BonusAccountModel>> GetAllAccountsAsync(int pageIndex, int pageSize)
    {
        return await _dbContext.BonusAccounts
            .OrderByDescending(b => b.Balance)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<BonusAccountModel> CreateAsync(BonusAccountModel account)
    {
        _dbContext.BonusAccounts.Add(account);
        await _dbContext.SaveChangesAsync();
        return account;
    }

    public async Task UpdateAsync(BonusAccountModel account)
    {
        account.UpdatedAt = DateTime.UtcNow;
        _dbContext.BonusAccounts.Update(account);
        await _dbContext.SaveChangesAsync();
    }

    public async Task AddTransactionAsync(BonusTransactionModel transaction)
    {
        _dbContext.BonusTransactions.Add(transaction);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<BonusTransactionModel>> GetTransactionsAsync(
        string userId, int pageIndex, int pageSize)
    {
        return await _dbContext.BonusTransactions
            .Where(t => t.BonusAccount.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<BonusTransactionModel>> GetTransactionsByOrderIdAsync(int orderId)
    {
        return await _dbContext.BonusTransactions
            .Where(t => t.OrderId == orderId)
            .ToListAsync();
    }

    public async Task<BonusSettingsModel> GetSettingsAsync()
    {
        return await _dbContext.BonusSettings.FirstAsync(s => s.Id == 1);
    }

    public async Task UpdateSettingsAsync(BonusSettingsModel settings)
    {
        settings.UpdatedAt = DateTime.UtcNow;
        _dbContext.BonusSettings.Update(settings);
        await _dbContext.SaveChangesAsync();
    }
}