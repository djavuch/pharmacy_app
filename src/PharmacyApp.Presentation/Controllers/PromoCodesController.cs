using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using PharmacyApp.Application.Contracts.PromoCode.Results;
using PharmacyApp.Application.Interfaces.Services;

namespace PharmacyApp.Presentation.Controllers;

[ApiController]
[EnableCors("AllowFrontend")]
public class PromoCodesController : ControllerBase
{
    private readonly IPromoCodeService _promoCodeService;
    
    public PromoCodesController(IPromoCodeService promoCodeService)
    {
        _promoCodeService = promoCodeService;
    }
    
    [HttpPost("validate")]
    public async Task<IActionResult> Validate([FromBody] PromoCodeValidationResults validationResults)
    {
        var result = await _promoCodeService.ValidatePromoCodeAsync(validationResults);

        if (!result.IsValid)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}