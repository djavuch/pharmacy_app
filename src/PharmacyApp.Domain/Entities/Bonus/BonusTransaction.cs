using PharmacyApp.Domain.Enums;

namespace PharmacyApp.Domain.Entities.Bonus;

public class BonusTransaction
{
    public Guid Id { get; set; }
    public Guid BonusAccountId { get; set; }
    public BonusAccount BonusAccount { get; set; } = null!;
    public BonusTransactionType Type { get; set; }
    public decimal Points { get; set; }
    public string Description { get; set; } = string.Empty;
    public int? OrderId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}