using PharmacyApp.Application.Contracts.Bonus;
using PharmacyApp.Application.Contracts.Bonus.Admin;
using PharmacyApp.Domain.Entities.Bonus;

namespace PharmacyApp.Application.Mappers;

public static class BonusMappers
{
    public static BonusAccountDto ToBonusAccountDto(this BonusAccount account) => new()
    {
        Id = account.Id,
        UserId = account.UserId,
        FullName = $"{account.User?.FirstName} {account.User?.LastName}".Trim(),
        Balance = account.Balance,
        CreatedAt = account.CreatedAt,
        UpdatedAt = account.UpdatedAt
    };

    public static BonusTransactionDto ToBonusTransactionDto(this BonusTransaction tx) => new()
    {
        Id = tx.Id,
        Type = tx.Type,
        Points = tx.Points,
        Description = tx.Description,
        OrderId = tx.OrderId,
        CreatedAt = tx.CreatedAt
    };

    public static BonusSettingsDto ToBonusSettingsDto(this BonusSettings settings) => new()
    {
        EarningRate = settings.EarningRate,
        MinOrderAmountToEarn = settings.MinOrderAmountToEarn,
        MaxRedeemPercent = settings.MaxRedeemPercent,
        IsEarningEnabled = settings.IsEarningEnabled,
        IsRedemptionEnabled = settings.IsRedemptionEnabled,
        UpdatedAt = settings.UpdatedAt
    };
}