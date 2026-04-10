namespace PharmacyApp.Domain.Entities.Bonus;

public class BonusAccount
{
    public Guid Id { get; private set; }
    public string UserId { get; set; } = string.Empty;
    public User User { get; set; } = null!;
    public decimal Balance { get; set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    public ICollection<BonusTransaction> Transactions { get; set; } = [];
    
    private BonusAccount() {}
    
    public BonusAccount(Guid id, string userId, decimal balance)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("UserId cannot be empty.");
        if (balance < 0)
            throw new ArgumentException("Balance cannot be negative.");

        Id = id;
        UserId = userId;
        Balance = balance;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void UpdateTimestamp()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}