using PharmacyApp.Application.DTOs.PromoCode;

namespace PharmacyApp.Application.Interfaces.Services;

public interface IPromoCodeService
{
    Task<PromoCodeDto> CreatePromoCodeAsync(CreatePromoCodeDto createPromoCodeDto);
    Task<PromoCodeDto?> GetPromoCodeByIdAsync(Guid promoCodeId);
    Task<PromoCodeDto?> GetPromoCodeByCodeAsync(string code);
    Task<IEnumerable<PromoCodeDto>> GetAllPromoCodesAsync();
    Task<IEnumerable<PromoCodeDto>> GetActivePromoCodesAsync();
    Task UpdatePromoCodeAsync(Guid promoCodeId, UpdatePromoCodeDto updatePromoCodeDto);
    Task DeletePromoCodeAsync(Guid promoCodeId);
    Task<PromoCodeValidationResultDto> ValidatePromoCodeAsync(ValidatePromoCodeDto validateDto);
}