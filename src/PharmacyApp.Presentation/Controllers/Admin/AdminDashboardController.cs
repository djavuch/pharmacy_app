using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyApp.Application.Common.Results;
using PharmacyApp.Application.Interfaces.Services;

namespace PharmacyApp.Presentation.Controllers.Admin;

[ApiController]
[Area("Admin")]
[Route("admin/dashboard")]
[Authorize(Roles = "Admin")]
public class AdminDashboardController : ControllerBase
{
    private readonly IAdminDashboardService _adminDashboardService;

    public AdminDashboardController(IAdminDashboardService adminDashboardService)
    {
        _adminDashboardService = adminDashboardService;
    }

    [HttpGet("summary")]
    public async Task<ApiResponse> GetSummary()
    {
        var summary = await _adminDashboardService.GetSummaryAsync();
        return new ApiResponse(true, null, summary);
    }
}
