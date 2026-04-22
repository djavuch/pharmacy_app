namespace PharmacyApp.Application.Contracts.Admin;

public record DashboardSummaryDto
{
    public int TotalProducts { get; init; }
    public int OrdersToday { get; init; }
    public decimal MonthlyRevenue { get; init; }
}
