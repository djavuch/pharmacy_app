using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using PharmacyApp.Application.Common.Pagination;
using PharmacyApp.Application.Contracts.Bonus.Admin;
using PharmacyApp.Application.Interfaces.Services;


namespace PharmacyApp.Presentation.Controllers.Admin;

[ApiController]
[EnableCors("AllowFrontend")]
[Route("admin/bonus")]
public class AdminBonusController : ControllerBase
{
    private readonly IBonusService _bonusService;

    public AdminBonusController(IBonusService bonusService)
    {
        _bonusService = bonusService;
    }

    [HttpGet("accounts/{userId}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetUserAccount(string userId)
    {
        return Ok(await _bonusService.GetOrCreateAccountAsync(userId));
    }

    // Transaction history for a user
    [HttpGet("accounts/{userId}/transactions")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetUserTransactions(
        string userId,
        [FromQuery] QueryParams queryParams)
    {
        return Ok(await _bonusService.GetTransactionsAsync(userId, queryParams));
    }

    // Manual adjustment bonus points for a user
    [HttpPost("accounts/{userId}/adjust")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdjustBonus(string userId, AdjustBonusDto dto)
    {
        var result = await _bonusService.AdminAdjustAsync(userId, dto);
        if (!result.IsSuccess)
            return StatusCode(result.ErrorType.ToStatusCode(), new { message = result.Message });
        return Ok(await _bonusService.GetOrCreateAccountAsync(userId));
    }

    // Update bonus settings
    [HttpPut("settings")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateSettings(UpdateBonusSettingsDto dto)
    {
        return Ok(await _bonusService.UpdateSettingsAsync(dto));
    }
}
