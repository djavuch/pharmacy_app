namespace PharmacyApp.Domain.Entities.PromoCode;

public class PromoCodeCategoryModel
{
    public Guid PromoCodeId { get; set; }
    public int CategoryId { get; set; }

    public PromoCodeModel PromoCode { get; set; } = null!;
}