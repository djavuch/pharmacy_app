using PharmacyApp.Domain.Enums;

namespace PharmacyApp.Domain.Entities.Bonus;

public class BonusTransaction
{
    public Guid Id { get; private set; }
    public Guid BonusAccountId { get; private set; }
    public BonusAccount BonusAccount { get; private set; } = null!;
    public BonusTransactionType Type { get; private set; }
    public decimal Points { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public int? OrderId { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private BonusTransaction()
    {
    }

    public static BonusTransaction CreateEarned(Guid bonusAccountId, decimal points, int orderId)
    {
        if (bonusAccountId == Guid.Empty)
            throw new ArgumentException("Bonus account ID cannot be empty.");
        if (orderId <= 0)
            throw new ArgumentException("Order ID must be greater than 0.");
        if (points <= 0)
            throw new ArgumentException("Earned points must be greater than 0.");

        return new BonusTransaction
        {
            Id = Guid.NewGuid(),
            BonusAccountId = bonusAccountId,
            Type = BonusTransactionType.Earned,
            Points = points,
            Description = $"Earned {points} points for order #{orderId}.",
            OrderId = orderId,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static BonusTransaction CreateRedeemed(Guid bonusAccountId, decimal points, decimal discount, int orderId)
    {
        if (bonusAccountId == Guid.Empty)
            throw new ArgumentException("Bonus account ID cannot be empty.");
        if (orderId <= 0)
            throw new ArgumentException("Order ID must be greater than 0.");
        if (points <= 0)
            throw new ArgumentException("Redeemed points must be greater than 0.");

        return new BonusTransaction
        {
            Id = Guid.NewGuid(),
            BonusAccountId = bonusAccountId,
            Type = BonusTransactionType.Redeemed,
            Points = points,
            Description = $"Used {points} points for order #{orderId} discount (−{discount}.)",
            OrderId = orderId,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static BonusTransaction CreateRefunded(
        Guid bonusAccountId,
        BonusTransactionType sourceType,
        decimal points,
        int orderId)
    {
        if (bonusAccountId == Guid.Empty)
            throw new ArgumentException("Bonus account ID cannot be empty.");
        if (orderId <= 0)
            throw new ArgumentException("Order ID must be greater than 0.");
        if (points <= 0)
            throw new ArgumentException("Refund points must be greater than 0.");
        if (sourceType is not BonusTransactionType.Earned and not BonusTransactionType.Redeemed)
            throw new ArgumentException("Only earned or redeemed transactions can be refunded.");

        return new BonusTransaction
        {
            Id = Guid.NewGuid(),
            BonusAccountId = bonusAccountId,
            Type = BonusTransactionType.Refunded,
            Points = points,
            Description = $"Refunded ({sourceType}) for order #{orderId} cancellation.",
            OrderId = orderId,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static BonusTransaction CreateAdminAdjustment(Guid bonusAccountId, decimal points, string reason)
    {
        if (bonusAccountId == Guid.Empty)
            throw new ArgumentException("Bonus account ID cannot be empty.");
        if (points == 0)
            throw new ArgumentException("Adjustment points cannot be 0.");

        var normalizedReason = string.IsNullOrWhiteSpace(reason) ? "No reason provided." : reason.Trim();

        return new BonusTransaction
        {
            Id = Guid.NewGuid(),
            BonusAccountId = bonusAccountId,
            Type = BonusTransactionType.AdminAdjustment,
            Points = points,
            Description = $"Admin adjustment: {normalizedReason}",
            CreatedAt = DateTime.UtcNow
        };
    }
}
