using PharmacyApp.Domain.Entities;

namespace PharmacyApp.Domain.Entities.Discount;

public class CategoryDiscount
{
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public Guid DiscountId { get; set; }
    public Discount Discount { get; set; } = null!;
}