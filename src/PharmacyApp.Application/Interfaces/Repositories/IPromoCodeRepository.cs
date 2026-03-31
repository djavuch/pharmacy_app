using PharmacyApp.Domain.Entities.PromoCode;

namespace PharmacyApp.Application.Interfaces.Repositories;

public interface IPromoCodeRepository
{
    Task<PromoCodeModel?> GetByIdAsync(Guid promoCodeId);
    Task<PromoCodeModel?> GetByCodeAsync(string code);
    Task<IEnumerable<PromoCodeModel>> GetAllAsync();
    Task<IEnumerable<PromoCodeModel>> GetActivePromoCodesAsync();
    Task<PromoCodeModel> AddAsync(PromoCodeModel promoCode);
    Task UpdateAsync(PromoCodeModel promoCode);
    Task DeleteAsync(Guid promoCodeId);
    Task<bool> CodeExistsAsync(string code, Guid? excludePromoCodeId = null);
    Task<int> GetUserUsageCountAsync(Guid promoCodeId, string userId);
    Task<int> IncrementUsageAsync(Guid promoCodeId);
    Task<int> DecrementUsageAsync(Guid promoCodeId);
    Task<int> RemoveUsageByOrderIdAsync(int orderId);
    Task RecordUsageAsync(PromoCodeUsageModel usage);
}