using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using PharmacyApp.Application.Interfaces.Services;
using System.Security.Claims;

namespace PharmacyApp.Presentation.Controllers;

[ApiController]
[EnableCors("AllowFrontend")]
[Route("bonus")]
[Authorize]
public class BonusController : ControllerBase
{
    private readonly IBonusService _bonusService;

    public BonusController(IBonusService bonusService)
    {
        _bonusService = bonusService;
    }

    [HttpGet("account")]
    public async Task<IActionResult> GetAccount()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        return Ok(await _bonusService.GetOrCreateAccountAsync(userId));
    }

    // Transaction history for the current user
    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions(int pageIndex = 1,  int pageSize = 10)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        return Ok(await _bonusService.GetTransactionsAsync(userId, pageIndex, pageSize));
    }

    // Get current bonus settings
    [HttpGet("settings")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSettings()
    {
        return Ok(await _bonusService.GetSettingsAsync());
    }
}