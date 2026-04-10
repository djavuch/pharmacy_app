using PharmacyApp.Domain.Entities;

namespace PharmacyApp.Domain.Entities.Discount;

public class ProductDiscount
{
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public Guid DiscountId { get; set; }
    public Discount Discount { get; set; } = null!;
}