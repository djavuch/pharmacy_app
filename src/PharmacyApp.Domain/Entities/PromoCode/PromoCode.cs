using PharmacyApp.Domain.Enums;

namespace PharmacyApp.Domain.Entities.PromoCode;

public class PromoCode
{
    public Guid PromoCodeId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public DiscountType DiscountType { get; private set; }
    public decimal Value { get; private set; }
    public DateTime StartDate { get; private set; } = DateTime.UtcNow;
    public DateTime EndDate { get; private set; }
    public bool IsActive { get; private set; } = true;

    public int? MaxUsageCount { get; private set; } // Max total usage count
    public int CurrentUsageCount { get; private set; } = 0; // Current total usage count
    public int? MaxUsagePerUser { get; private set; } // Max usage per user

    public decimal? MinimumOrderAmount { get; private set; }
    public decimal? MaximumDiscountAmount { get;private  set; }

    public bool ApplicableToAllProducts { get; private set; } = false;

    public List<PromoCodeProduct> PromoCodeProducts { get; set; } = new();
    public List<PromoCodeCategory> PromoCodeCategories { get; set; } = [];
    public List<PromoCodeUsage> UsageHistory { get; set; } = [];

    private PromoCode() { }

    public PromoCode(string code, string description, DiscountType discountType,
        decimal value, DateTime startDate, DateTime endDate,
        bool applicableToAllProducts = false,
        int? maxUsageCount = null, int? maxUsagePerUser = null,
        decimal? minimumOrderAmount = null, decimal? maximumDiscountAmount = null)
    {
        Code = code;
        Description = description;
        DiscountType = discountType;
        Value = value;
        StartDate = startDate;
        EndDate = endDate;
        ApplicableToAllProducts = applicableToAllProducts;
        MaxUsageCount = maxUsageCount;
        MaxUsagePerUser = maxUsagePerUser;
        MinimumOrderAmount = minimumOrderAmount;
        MaximumDiscountAmount = maximumDiscountAmount;
        IsActive = true;
        PromoCodeId = Guid.NewGuid();

        ValidateBusinessRules();
    }
    
    public void Update(string code, string description, DiscountType discountType,
        decimal value, DateTime startDate, DateTime endDate,
        bool applicableToAllProducts = false,
        int? maxUsageCount = null, int? maxUsagePerUser = null,
        decimal? minimumOrderAmount = null, decimal? maximumDiscountAmount = null)
    {
        Code = code;
        Description = description;
        DiscountType = discountType;
        Value = value;
        StartDate = startDate;
        EndDate = endDate;
        ApplicableToAllProducts = applicableToAllProducts;
        MaxUsageCount = maxUsageCount;
        MaxUsagePerUser = maxUsagePerUser;
        MinimumOrderAmount = minimumOrderAmount;
        MaximumDiscountAmount = maximumDiscountAmount;

        ValidateBusinessRules();
    }
    
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

    public void Activate()
    {
        if (IsActive)
            throw new InvalidOperationException("This promo code is already active.");
        IsActive = true;
    }

    public void Deactivate()
    {
        if (!IsActive)
            throw new InvalidOperationException("This promo code is not active.");
        IsActive = false;
    }
    
    public void UpdateApplicableProducts(IEnumerable<int>? productIds, IEnumerable<int>? categoryIds)
    {
        PromoCodeProducts.Clear();
        PromoCodeCategories.Clear();

        if (ApplicableToAllProducts) return;

        if (productIds?.Any() == true)
            PromoCodeProducts.AddRange(
                productIds.Select(pid => new PromoCodeProduct { ProductId = pid, PromoCodeId = PromoCodeId }));

        if (categoryIds?.Any() == true)
            PromoCodeCategories.AddRange(
                categoryIds.Select(cid => new PromoCodeCategory { CategoryId = cid, PromoCodeId = PromoCodeId }));
    }
}