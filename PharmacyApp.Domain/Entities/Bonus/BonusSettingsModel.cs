namespace PharmacyApp.Domain.Entities.Bonus;

public class BonusSettingsModel
{
    public int Id { get; set; } = 1;
    // How many bonus points are earned per 1 unit of currency spent
    public decimal EarningRate { get; set; } = 1m;
    public decimal MinOrderAmountToEarn { get; set; } = 0m;
    public decimal MaxRedeemPercent { get; set; } = 100m;
    // Is the bonus program enabled for earning points
    public bool IsEarningEnabled { get; set; } = true;
    // Is the bonus program enabled for redeeming points
    public bool IsRedemptionEnabled { get; set; } = true;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}