using PharmacyApp.Domain.Entities.Bonus;
using PharmacyApp.Domain.Enums;

namespace PharmacyApp.UnitTests.Domain;

public sealed class BonusAccountTests
{
    [Fact]
    public void Constructor_WhenUserIdIsEmpty_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new BonusAccount(Guid.NewGuid(), "", 0m));
    }

    [Fact]
    public void Constructor_WhenBalanceIsNegative_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new BonusAccount(Guid.NewGuid(), "user-1", -1m));
    }

    [Fact]
    public void EarnForOrder_WhenValuesAreValid_IncreasesBalanceAndCreatesTransaction()
    {
        var account = CreateAccount(balance: 5m);

        var transaction = account.EarnForOrder(orderId: 10, points: 2.5m);

        Assert.Equal(7.5m, account.Balance);
        Assert.Equal(BonusTransactionType.Earned, transaction.Type);
        Assert.Equal(2.5m, transaction.Points);
        Assert.Equal(10, transaction.OrderId);
    }

    [Fact]
    public void RedeemForOrder_WhenBalanceIsEnough_DecreasesBalanceAndCreatesTransaction()
    {
        var account = CreateAccount(balance: 10m);

        var transaction = account.RedeemForOrder(orderId: 10, points: 3m, discount: 3m);

        Assert.Equal(7m, account.Balance);
        Assert.Equal(BonusTransactionType.Redeemed, transaction.Type);
        Assert.Equal(3m, transaction.Points);
        Assert.Equal(10, transaction.OrderId);
    }

    [Fact]
    public void RedeemForOrder_WhenBalanceIsInsufficient_ThrowsInvalidOperationException()
    {
        var account = CreateAccount(balance: 2m);

        Assert.Throws<InvalidOperationException>(() =>
            account.RedeemForOrder(orderId: 10, points: 3m, discount: 3m));
    }

    [Fact]
    public void ApplyAdminAdjustment_WhenPositive_IncreasesBalance()
    {
        var account = CreateAccount(balance: 2m);

        var transaction = account.ApplyAdminAdjustment(5m, "Compensation");

        Assert.Equal(7m, account.Balance);
        Assert.Equal(BonusTransactionType.AdminAdjustment, transaction.Type);
        Assert.Contains("Compensation", transaction.Description);
    }

    [Fact]
    public void ApplyAdminAdjustment_WhenResultWouldBeNegative_ThrowsInvalidOperationException()
    {
        var account = CreateAccount(balance: 2m);

        Assert.Throws<InvalidOperationException>(() =>
            account.ApplyAdminAdjustment(-3m, "Correction"));
    }

    [Fact]
    public void ReverseOrderTransactions_WhenEarnedTransactionExists_SubtractsEarnedPoints()
    {
        var account = CreateAccount(balance: 0m);
        var earned = account.EarnForOrder(orderId: 10, points: 5m);

        var refunds = account.ReverseOrderTransactions(10, [earned]);

        Assert.Equal(0m, account.Balance);
        var refund = Assert.Single(refunds);
        Assert.Equal(BonusTransactionType.Refunded, refund.Type);
        Assert.Equal(5m, refund.Points);
    }

    [Fact]
    public void ReverseOrderTransactions_WhenRedeemedTransactionExists_ReturnsRedeemedPoints()
    {
        var account = CreateAccount(balance: 10m);
        var redeemed = account.RedeemForOrder(orderId: 10, points: 4m, discount: 4m);

        var refunds = account.ReverseOrderTransactions(10, [redeemed]);

        Assert.Equal(10m, account.Balance);
        var refund = Assert.Single(refunds);
        Assert.Equal(BonusTransactionType.Refunded, refund.Type);
        Assert.Equal(4m, refund.Points);
    }

    [Fact]
    public void CreateAdminAdjustment_WhenReasonIsEmpty_UsesFallbackReason()
    {
        var transaction = BonusTransaction.CreateAdminAdjustment(Guid.NewGuid(), 5m, "");

        Assert.Contains("No reason provided.", transaction.Description);
    }

    private static BonusAccount CreateAccount(decimal balance) =>
        new(Guid.NewGuid(), "user-1", balance);
}
