using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using PharmacyApp.Application.Interfaces.Services;

namespace PharmacyApp.Presentation.Controllers;

[ApiController]
[EnableCors("AllowFrontend")]
[Route("promotions")]
public class PromotionController : ControllerBase
{
    private readonly IDiscountService _discountService;

    public PromotionController(IDiscountService discountService)
    {
        _discountService = discountService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPromotions()
    {
        var promotions = await _discountService.GetActivePromotionsAsync();
        return Ok(promotions);
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetPromotionBySlug(string slug)
    {
        var result = await _discountService.GetActivePromotionBySlugAsync(slug);
        if (!result.IsSuccess)
            return StatusCode(result.ErrorType.ToStatusCode(), new { message = result.Message });

        return Ok(result.Value);
    }
}
