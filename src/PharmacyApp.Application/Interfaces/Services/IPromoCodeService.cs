using PharmacyApp.Application.Contracts.PromoCode;
using PharmacyApp.Application.Contracts.PromoCode.Results;
using PharmacyApp.Domain.Common;

namespace PharmacyApp.Application.Interfaces.Services;

public interface IPromoCodeService
{
    Task<Result<PromoCodeDto>> CreatePromoCodeAsync(CreatePromoCodeDto createPromoCodeDto);
    Task<PromoCodeDto?> GetPromoCodeByIdAsync(Guid promoCodeId);
    Task<PromoCodeDto?> GetPromoCodeByCodeAsync(string code);
    Task<IEnumerable<PromoCodeDto>> GetAllPromoCodesAsync();
    Task<IEnumerable<PromoCodeDto>> GetActivePromoCodesAsync();
    Task<Result> UpdatePromoCodeAsync(Guid promoCodeId, UpdatePromoCodeDto updatePromoCodeDto);
    Task<Result> DeletePromoCodeAsync(Guid promoCodeId);
    Task<PromoCodeValidationResultDto> ValidatePromoCodeAsync(PromoCodeValidationResults validationResults);
    Task<Result> ActivatePromoCodeAsync(Guid promoCodeId);
    Task<Result> DeactivatePromoCodeAsync(Guid promoCodeId);
    Task RefreshPromoCodeUsageCacheAsync(Guid promoCodeId, string? code = null);
}
