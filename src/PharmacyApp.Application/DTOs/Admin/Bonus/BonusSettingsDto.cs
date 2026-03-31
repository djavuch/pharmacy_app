namespace PharmacyApp.Application.DTOs.Admin.Bonus;

public record BonusSettingsDto
{
    public decimal EarningRate { get; set; }
    public decimal MinOrderAmountToEarn { get; set; }
    public decimal MaxRedeemPercent { get; set; }
    public bool IsEarningEnabled { get; set; }
    public bool IsRedemptionEnabled { get; set; }
    public DateTime UpdatedAt { get; set; }
}