namespace PharmacyApp.Application.Contracts.Bonus.Admin;

public record AdjustBonusDto
{
    public decimal Points { get; set; }
    public string Reason { get; set; } = string.Empty;
}