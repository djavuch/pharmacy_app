using PharmacyApp.Domain.Enums;

namespace PharmacyApp.Domain.Entities.PromoCode;

public class PromoCodeModel
{
    public Guid PromoCodeId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DiscountType DiscountType { get; set; }
    public decimal Value { get; set; }
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; } = true;

    public int? MaxUsageCount { get; set; } // Max total usage count
    public int CurrentUsageCount { get; set; } = 0; // Current total usage count
    public int? MaxUsagePerUser { get; set; } // Max usage per user

    public decimal? MinimumOrderAmount { get; set; }
    public decimal? MaximumDiscountAmount { get; set; }

    public bool ApplicableToAllProducts { get; set; } = false;

    // Навигационные свойства
    public ICollection<PromoCodeProductModel> PromoCodeProducts { get; set; } = [];
    public ICollection<PromoCodeCategoryModel> PromoCodeCategories { get; set; } = [];
    public ICollection<PromoCodeUsageModel> UsageHistory { get; set; } = [];

    public void ValidateBusinessRules()
    {
        if (string.IsNullOrWhiteSpace(Code))
            throw new ArgumentException("Promo code cannot be empty.");

        if (Code.Length < 3)
            throw new ArgumentException("Promo code must be at least 3 characters long.");

        if (Value <= 0)
            throw new ArgumentException("Discount value must be greater than 0.");

        if (DiscountType == DiscountType.Percentage && Value > 100)
            throw new ArgumentException("Percentage discount cannot exceed 100%.");

        if (EndDate <= StartDate)
            throw new ArgumentException("End date must be after start date.");

        if (MaxUsageCount.HasValue && MaxUsageCount.Value <= 0)
            throw new ArgumentException("Max usage count must be greater than 0.");

        if (MaxUsagePerUser.HasValue && MaxUsagePerUser.Value <= 0)
            throw new ArgumentException("Max usage per user must be greater than 0.");
    }

    public bool IsValid()
    {
        var now = DateTime.UtcNow;
        return IsActive
            && now >= StartDate
            && now <= EndDate
            && (!MaxUsageCount.HasValue || CurrentUsageCount < MaxUsageCount.Value);
    }

    public bool CanBeUsedByUser(string userId)
    {
        if (!MaxUsagePerUser.HasValue)
            return true;

        var userUsageCount = UsageHistory.Count(u => u.UserId == userId);
        return userUsageCount < MaxUsagePerUser.Value;
    }
}