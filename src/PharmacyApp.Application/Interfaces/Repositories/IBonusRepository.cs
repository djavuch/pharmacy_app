using PharmacyApp.Domain.Entities.Bonus;

namespace PharmacyApp.Application.Interfaces.Repositories;

public interface IBonusRepository
{
    Task<BonusAccountModel?> GetByUserIdAsync(string userId);
    Task<IEnumerable<BonusAccountModel>> GetAllAccountsAsync(int pageIndex, int pageSize);
    Task<BonusAccountModel> CreateAsync(BonusAccountModel account);
    Task UpdateAsync(BonusAccountModel account);
    Task AddTransactionAsync(BonusTransactionModel transaction);
    Task<IEnumerable<BonusTransactionModel>> GetTransactionsAsync(string userId, int pageIndex, int pageSize);
    Task<IEnumerable<BonusTransactionModel>> GetTransactionsByOrderIdAsync(int orderId);

    // Settings
    Task<BonusSettingsModel> GetSettingsAsync();
    Task UpdateSettingsAsync(BonusSettingsModel settings);
}