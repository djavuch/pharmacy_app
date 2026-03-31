namespace PharmacyApp.Domain.Entities.PromoCode;

public class PromoCodeProductModel
{
    public Guid PromoCodeId { get; set; }
    public int ProductId { get; set; }

    public PromoCodeModel PromoCode { get; set; } = null!;
}