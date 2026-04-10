namespace PharmacyApp.Domain.Entities.PromoCode;

public class PromoCodeProduct
{
    public Guid PromoCodeId { get; set; }
    public int ProductId { get; set; }

    public PromoCode PromoCode { get; set; } = null!;
}