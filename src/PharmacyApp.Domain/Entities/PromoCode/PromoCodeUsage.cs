namespace PharmacyApp.Domain.Entities.PromoCode;

public class PromoCodeUsage
{
    public Guid UsageId { get; private set; }
    public Guid PromoCodeId { get; private set; }
    public string UserId { get; private set; }
    public int OrderId { get; private set; }
    public decimal DiscountApplied { get; private set; }
    public DateTime UsedAt { get; private set; } = DateTime.UtcNow;

    public PromoCode PromoCode { get; set; } = null!;
    
    private PromoCodeUsage() { }

    public PromoCodeUsage(Guid promoCodeId, string userId, int orderId, decimal discountApplied)
    {
        UsageId = Guid.NewGuid();
        PromoCodeId = promoCodeId;
        UserId = userId;
        OrderId = orderId;
        DiscountApplied = discountApplied;
        UsedAt = DateTime.UtcNow;
    }
}