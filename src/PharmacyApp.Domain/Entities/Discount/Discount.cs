using PharmacyApp.Domain.Enums;

namespace PharmacyApp.Domain.Entities.Discount;

public class Discount
{
    public Guid DiscountId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private  set; } = string.Empty;
    public DiscountType DiscountType { get; private set; }
    public decimal Value { get; private set; }
    public DateTime StartDate { get; private set; } = DateTime.UtcNow;
    public DateTime EndDate { get; private set; } = DateTime.UtcNow;
    public bool IsActive { get; private set; } 

    public decimal? MinimumOrderAmount { get; private set; }
    public decimal? MaximumOrderAmount { get; private set; }

    public ICollection<ProductDiscount> ProductDiscounts { get; set; } = new List<ProductDiscount>();
    public ICollection<CategoryDiscount> CategoryDiscounts { get; set; } = new List<CategoryDiscount>();

    private Discount() { }

    public Discount(string name, string description, DiscountType discountType,
        decimal value, DateTime startDate, DateTime endDate,
        decimal? minimumOrderAmount = null, decimal? maximumOrderAmount = null)
    {
        Name = name;
        Description = description;
        DiscountType = discountType;
        Value = value;
        StartDate = startDate;
        EndDate = endDate;
        MinimumOrderAmount = minimumOrderAmount;
        MaximumOrderAmount = maximumOrderAmount;
        IsActive = true;

        ValidateBusinessRules();
    }

    public void Update(string name, string description, DiscountType discountType,
        decimal value, DateTime startDate, DateTime endDate,
        decimal? minimumOrderAmount = null, decimal? maximumOrderAmount = null)
    {
        Name = name;
        Description = description;
        DiscountType = discountType;
        Value = value;
        StartDate = startDate;
        EndDate = endDate;
        MinimumOrderAmount = minimumOrderAmount;
        MaximumOrderAmount = maximumOrderAmount;

        ValidateBusinessRules();
    }

    private void ValidateBusinessRules()
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