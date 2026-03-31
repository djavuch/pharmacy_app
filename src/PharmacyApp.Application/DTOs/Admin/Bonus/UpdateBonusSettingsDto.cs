using System.ComponentModel.DataAnnotations;

namespace PharmacyApp.Application.DTOs.Admin.Bonus;

public record UpdateBonusSettingsDto
{
    [Range(0.01, 100)]
    public decimal EarningRate { get; set; }

    [Range(0, 100)]
    public decimal MinOrderAmountToEarn { get; set; }

    [Range(1, 100)]
    public decimal MaxRedeemPercent { get; set; }
    public bool IsEarningEnabled { get; set; }
    public bool IsRedemptionEnabled { get; set; }
}