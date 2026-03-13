using PharmacyApp.Domain.Entities;

namespace PharmacyApp.Domain.Entities.Discount;

public class CategoryDiscountModel
{
    public int CategoryId { get; set; }
    public CategoryModel Category { get; set; } = null!;

    public Guid DiscountId { get; set; }
    public DiscountModel Discount { get; set; } = null!;
}