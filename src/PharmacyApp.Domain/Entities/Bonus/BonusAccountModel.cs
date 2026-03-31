namespace PharmacyApp.Domain.Entities.Bonus;

public class BonusAccountModel
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public UserModel User { get; set; } = null!;
    public decimal Balance { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<BonusTransactionModel> Transactions { get; set; } = [];
}