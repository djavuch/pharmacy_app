using PharmacyApp.Domain.Entities.Bonus;

namespace PharmacyApp.Application.Interfaces.Repositories;

public interface IBonusRepository
{
    Task<BonusAccount?> GetByUserIdAsync(string userId);
    Task<IEnumerable<BonusAccount>> GetAllAccountsAsync(int pageIndex, int pageSize);
    Task<BonusAccount> CreateAsync(BonusAccount account);
    Task UpdateAsync(BonusAccount account);
    Task AddTransactionAsync(BonusTransaction transaction);
    Task<IEnumerable<BonusTransaction>> GetTransactionsAsync(string userId, int pageIndex, int pageSize);
    Task<IEnumerable<BonusTransaction>> GetTransactionsByOrderIdAsync(int orderId);

    // Settings
    Task<BonusSettings> GetSettingsAsync();
    Task UpdateSettingsAsync(BonusSettings settings);
}