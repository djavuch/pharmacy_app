using PharmacyApp.Application.Contracts.Admin;

namespace PharmacyApp.Application.Interfaces.Services;

public interface IAdminDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync();
}
