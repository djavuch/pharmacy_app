using PharmacyApp.Domain.Enums;

namespace PharmacyApp.Domain.Entities.Bonus;

public class BonusTransactionModel
{
    public Guid Id { get; set; }
    public Guid BonusAccountId { get; set; }
    public BonusAccountModel BonusAccount { get; set; } = null!;
    public BonusTransactionType Type { get; set; }
    public decimal Points { get; set; }
    public string Description { get; set; } = string.Empty;
    public int? OrderId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}