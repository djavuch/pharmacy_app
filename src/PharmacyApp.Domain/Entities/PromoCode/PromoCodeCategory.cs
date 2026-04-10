namespace PharmacyApp.Domain.Entities.PromoCode;

public class PromoCodeCategory
{
    public Guid PromoCodeId { get; set; }
    public int CategoryId { get; set; }

    public PromoCode PromoCode { get; set; } = null!;
}