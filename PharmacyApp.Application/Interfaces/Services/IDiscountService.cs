using PharmacyApp.Application.DTOs.Discount;

namespace PharmacyApp.Application.Interfaces.Services;

public interface IDiscountService
{
    Task<DiscountDto> CreateDiscountAsync(CreateDiscountDto dto);
    Task<DiscountDto?> GetDiscountByIdAsync(Guid discountId);
    Task<IEnumerable<DiscountDto>> GetAllDiscountsAsync();
    Task<IEnumerable<DiscountDto>> GetActiveDiscountsAsync();
    Task UpdateDiscountAsync(Guid discountId, UpdateDiscountDto dto);
    Task DeleteDiscountAsync(Guid discountId);
    Task<decimal> CalculateDiscountedPriceAsync(int productId, decimal originalPrice);
}
