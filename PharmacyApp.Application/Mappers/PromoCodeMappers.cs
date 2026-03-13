using PharmacyApp.Application.DTOs.PromoCode;
using PharmacyApp.Domain.Entities.PromoCode;

namespace PharmacyApp.Application.Mappers;

public static class PromoCodeMapperExtensions
{
    public static PromoCodeDto ToPromoCodeDto(this PromoCodeModel promoCode) => new()
    {
        PromoCodeId = promoCode.PromoCodeId,
        Code = promoCode.Code,
        Description = promoCode.Description,
        DiscountType = promoCode.DiscountType.ToString(),
        Value = promoCode.Value,
        StartDate = promoCode.StartDate,
        EndDate = promoCode.EndDate,
        IsActive = promoCode.IsActive,
        MaxUsageCount = promoCode.MaxUsageCount,
        CurrentUsageCount = promoCode.CurrentUsageCount,
        MaxUsagePerUser = promoCode.MaxUsagePerUser,
        MinimumOrderAmount = promoCode.MinimumOrderAmount,
        MaximumDiscountAmount = promoCode.MaximumDiscountAmount,
        ApplicableToAllProducts = promoCode.ApplicableToAllProducts,
        ProductIds = promoCode.PromoCodeProducts.Select(p => p.ProductId).ToList(),
        CategoryIds = promoCode.PromoCodeCategories.Select(c => c.CategoryId).ToList()
    };
}