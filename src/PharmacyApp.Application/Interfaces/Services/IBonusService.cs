using PharmacyApp.Application.DTOs.Admin.Bonus;
using PharmacyApp.Application.DTOs.Bonus;

namespace PharmacyApp.Application.Interfaces.Services;

public interface IBonusService
{
    Task<BonusAccountDto> GetOrCreateAccountAsync(string userId);
    Task<IEnumerable<BonusTransactionDto>> GetTransactionsAsync(string userId, int pageIndex = 1, int pageSize = 20);

    // Earn bonus points for an order based on the paid amount.
    Task<decimal> EarnPointsAsync(string userId, int orderId, decimal paidAmount);

    // Use bonus points to get a discount on an order.
    Task<decimal> RedeemPointsAsync(string userId, int orderId, decimal pointsToRedeem);
    // Refund bonuses if an order is canceled
    Task ReverseOrderBonusesAsync(string userId, int orderId);

    // Admin accounts 
    Task<IEnumerable<BonusAccountDto>> GetAllAccountsAsync(int pageIndex = 1, int pageSize = 20);
    Task AdminAdjustAsync(string userId, AdminAdjustBonusDto dto);

    // Admin settings
    Task<BonusSettingsDto> GetSettingsAsync();
    Task<BonusSettingsDto> UpdateSettingsAsync(UpdateBonusSettingsDto dto);
}