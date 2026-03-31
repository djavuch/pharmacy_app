using PharmacyApp.Domain.Entities;

namespace PharmacyApp.Domain.Entities.Discount;

public class ProductDiscountModel
{
    public int ProductId { get; set; }
    public ProductModel Product { get; set; } = null!;

    public Guid DiscountId { get; set; }
    public DiscountModel Discount { get; set; } = null!;
}