using PharmacyApp.Application.DTOs.Discount;
using PharmacyApp.Domain.Entities.Discount;

namespace PharmacyApp.Application.Mappers;

public static class DiscountMappers
{
    public static DiscountDto ToDiscountDto(this DiscountModel discount) => new()
    {
        DiscountId = discount.DiscountId,
        Name = discount.Name,
        Description = discount.Description,
        DiscountType = discount.DiscountType,
        Value = discount.Value,
        StartDate = discount.StartDate,
        EndDate = discount.EndDate,
        IsActive = discount.IsActive,
        MinimumOrderAmount = discount.MinimumOrderAmount,
        MaximumOrderAmount = discount.MaximumOrderAmount,
        ProductIds = discount.ProductDiscounts?.Select(pd => pd.ProductId).ToList() ?? [],
        CategoryIds = discount.CategoryDiscounts?.Select(cd => cd.CategoryId).ToList() ?? []
    };
}
