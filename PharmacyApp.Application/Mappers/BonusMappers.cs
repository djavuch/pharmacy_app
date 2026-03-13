using PharmacyApp.Application.DTOs.Bonus;
using PharmacyApp.Domain.Entities.Bonus;

namespace PharmacyApp.Application.Mappers;

public static class BonusMappers
{
    public static BonusAccountDto ToBonusAccountDto(this BonusAccountModel account) => new()
    {
        Id = account.Id,
        UserId = account.UserId,
        UserFullName = account.User is not null
            ? $"{account.User.FirstName} {account.User.LastName}"
            : null,
        Balance = account.Balance,
        CreatedAt = account.CreatedAt,
        UpdatedAt = account.UpdatedAt
    };

    public static BonusTransactionDto ToBonusTransactionDto(this BonusTransactionModel tx) => new()
    {
        Id = tx.Id,
        Type = tx.Type,
        Points = tx.Points,
        Description = tx.Description,
        OrderId = tx.OrderId,
        CreatedAt = tx.CreatedAt
    };

    public static BonusSettingsDto ToBonusSettingsDto(this BonusSettingsModel settings) => new()
    {
        EarningRate = settings.EarningRate,
        MinOrderAmountToEarn = settings.MinOrderAmountToEarn,
        MaxRedeemPercent = settings.MaxRedeemPercent,
        IsEarningEnabled = settings.IsEarningEnabled,
        IsRedemptionEnabled = settings.IsRedemptionEnabled,
        UpdatedAt = settings.UpdatedAt
    };
}