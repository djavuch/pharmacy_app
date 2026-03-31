using PharmacyApp.Domain.Enums;

namespace PharmacyApp.Domain.Entities.Discount;

public class DiscountModel
{
    public Guid DiscountId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DiscountType DiscountType { get; set; }
    public decimal Value { get; set; }
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime EndDate { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } 

    public decimal? MinimumOrderAmount { get; set; }
    public decimal? MaximumOrderAmount { get; set; }

    public ICollection<ProductDiscountModel> ProductDiscounts { get; set; } = new List<ProductDiscountModel>();
    public ICollection<CategoryDiscountModel> CategoryDiscounts { get; set; } = new List<CategoryDiscountModel>();

    public void ValidateBusinessRules()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new ArgumentException("Discount name cannot be empty.");

        if (Value <= 0)
            throw new ArgumentException("Discount value must be greater than 0.");

        if (DiscountType == DiscountType.Percentage && Value > 100)
            throw new ArgumentException("Percentage discount cannot exceed 100%.");

        if (EndDate <= StartDate)
            throw new ArgumentException("End date must be after start date.");
    }

    public bool isValid()
    {
        var now = DateTime.UtcNow;
        return IsActive && now >= StartDate && now <= EndDate;
    }
}