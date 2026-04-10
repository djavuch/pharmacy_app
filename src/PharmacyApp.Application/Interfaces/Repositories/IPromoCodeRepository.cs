using PharmacyApp.Domain.Entities.PromoCode;

namespace PharmacyApp.Application.Interfaces.Repositories;

public interface IPromoCodeRepository
{
    Task<PromoCode?> GetByIdAsync(Guid promoCodeId);
    Task<PromoCode?> GetByCodeAsync(string code);
    Task<IEnumerable<PromoCode>> GetAllAsync();
    Task<IEnumerable<PromoCode>> GetActivePromoCodesAsync();
    Task<PromoCode> AddAsync(PromoCode promoCode);
    Task UpdateAsync(PromoCode promoCode);
    Task DeleteAsync(Guid promoCodeId);
    Task<bool> CodeExistsAsync(string code, Guid? excludePromoCodeId = null);
    Task<int> GetUserUsageCountAsync(Guid promoCodeId, string userId);
    Task<int> IncrementUsageAsync(Guid promoCodeId);
    Task<int> DecrementUsageAsync(Guid promoCodeId);
    Task<int> RemoveUsageByOrderIdAsync(int orderId);
    Task RecordUsageAsync(PromoCodeUsage usage);
}