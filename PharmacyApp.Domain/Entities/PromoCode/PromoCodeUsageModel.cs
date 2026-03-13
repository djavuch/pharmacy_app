namespace PharmacyApp.Domain.Entities.PromoCode;

public class PromoCodeUsageModel
{
    public Guid UsageId { get; set; }
    public Guid PromoCodeId { get; set; }
    public string UserId { get; set; }
    public int OrderId { get; set; }
    public decimal DiscountApplied { get; set; }
    public DateTime UsedAt { get; set; } = DateTime.UtcNow;

    public PromoCodeModel PromoCode { get; set; } = null!;
}