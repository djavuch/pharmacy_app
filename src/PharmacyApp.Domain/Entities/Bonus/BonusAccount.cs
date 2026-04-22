using PharmacyApp.Domain.Enums;

namespace PharmacyApp.Domain.Entities.Bonus;

public class BonusAccount
{
    public Guid Id { get; private set; }
    public string UserId { get; set; } = string.Empty;
    public User User { get; set; } = null!;
    public decimal Balance { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    public ICollection<BonusTransaction> Transactions { get; set; } = [];

    private BonusAccount()
    {
    }

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

    public BonusTransaction EarnForOrder(int orderId, decimal points)
    {
        if (orderId <= 0)
            throw new ArgumentException("Order ID must be greater than 0.");
        if (points <= 0)
            throw new ArgumentException("Earned points must be greater than 0.");

        Balance += points;
        UpdateTimestamp();

        return BonusTransaction.CreateEarned(Id, points, orderId);
    }

    public BonusTransaction RedeemForOrder(int orderId, decimal points, decimal discount)
    {
        if (orderId <= 0)
            throw new ArgumentException("Order ID must be greater than 0.");
        if (points <= 0)
            throw new ArgumentException("Points to redeem must be greater than 0.");
        if (points > Balance)
            throw new InvalidOperationException(
                $"Not enough points. Available: {Balance}, requested: {points}.");

        var normalizedDiscount = Math.Round(discount, 2);
        Balance -= points;
        UpdateTimestamp();

        return BonusTransaction.CreateRedeemed(Id, points, normalizedDiscount, orderId);
    }

    public IReadOnlyList<BonusTransaction> ReverseOrderTransactions(
        int orderId,
        IEnumerable<BonusTransaction> transactions)
    {
        if (orderId <= 0)
            throw new ArgumentException("Order ID must be greater than 0.");
        if (transactions is null)
            throw new ArgumentNullException(nameof(transactions));

        var refundTransactions = new List<BonusTransaction>();

        foreach (var tx in transactions)
        {
            switch (tx.Type)
            {
                case BonusTransactionType.Earned:
                    Balance -= tx.Points;
                    refundTransactions.Add(BonusTransaction.CreateRefunded(
                        Id,
                        BonusTransactionType.Earned,
                        tx.Points,
                        orderId));
                    break;
                
                case BonusTransactionType.Redeemed:
                    Balance += tx.Points;
                    refundTransactions.Add(BonusTransaction.CreateRefunded(
                        Id,
                        BonusTransactionType.Redeemed,
                        tx.Points,
                        orderId));
                    break;
            }
        }

        Balance = Math.Max(0, Balance);

        if (refundTransactions.Count > 0)
            UpdateTimestamp();

        return refundTransactions;
    }

    public BonusTransaction ApplyAdminAdjustment(decimal points, string reason)
    {
        if (points == 0)
            throw new ArgumentException("Points cannot be 0.");

        if (Balance + points < 0)
            throw new InvalidOperationException(
                $"Adjustment would result in negative balance. Current: {Balance}, delta: {points}.");

        Balance += points;
        UpdateTimestamp();

        return BonusTransaction.CreateAdminAdjustment(Id, points, reason);
    }

    public void UpdateTimestamp()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}
