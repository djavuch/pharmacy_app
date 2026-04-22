using PharmacyApp.Application.Common;
using PharmacyApp.Application.Contracts.Discount;
using PharmacyApp.Application.Contracts.Promotion;
using PharmacyApp.Domain.Common;

namespace PharmacyApp.Application.Interfaces.Services;

public interface IDiscountService
{
    Task<Result<DiscountDto>> CreateDiscountAsync(CreateDiscountDto dto);
    Task<DiscountDto?> GetDiscountByIdAsync(Guid discountId);
    Task<IEnumerable<DiscountDto>> GetAllDiscountsAsync();
    Task<IEnumerable<DiscountDto>> GetActiveDiscountsAsync();
    Task<IReadOnlyCollection<PromotionListItemDto>> GetActivePromotionsAsync();
    Task<Result<PromotionDetailsDto>> GetActivePromotionBySlugAsync(string slug);
    Task<Result> UpdateDiscountAsync(Guid discountId, UpdateDiscountDto dto);
    Task<Result> DeleteDiscountAsync(Guid discountId);
    Task<decimal> CalculateDiscountedPriceAsync(int productId, int categoryId, decimal originalPrice);
    Task<Dictionary<int, decimal>> CalculateDiscountedPricesAsync(
        IReadOnlyCollection<ProductPriceContext> products);
}
