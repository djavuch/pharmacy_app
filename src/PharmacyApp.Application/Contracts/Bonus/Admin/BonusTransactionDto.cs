using PharmacyApp.Domain.Enums;

namespace PharmacyApp.Application.Contracts.Bonus.Admin;

public record BonusTransactionDto
{
    public Guid Id { get; set; }
    public BonusTransactionType Type { get; set; }
    public decimal Points { get; set; }
    public string Description { get; set; } = string.Empty;
    public int? OrderId { get; set; }
    public DateTime CreatedAt { get; set; }
}