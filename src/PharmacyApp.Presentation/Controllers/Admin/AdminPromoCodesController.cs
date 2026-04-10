using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyApp.Application.Contracts.PromoCode;
using PharmacyApp.Application.Contracts.PromoCode.Results;
using PharmacyApp.Application.Interfaces.Services;

namespace PharmacyApp.Presentation.Controllers.Admin;

[ApiController]
[Route("admin/promo-codes")]
[Authorize(Roles = "Admin")]
public class PromoCodeController : ControllerBase
{
    private readonly IPromoCodeService _promoCodeService;

    public PromoCodeController(IPromoCodeService promoCodeService)
    {
        _promoCodeService = promoCodeService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var promoCodes = await _promoCodeService.GetAllPromoCodesAsync();
        return Ok(promoCodes);
    }

    [HttpGet("active")]
    [AllowAnonymous]
    public async Task<IActionResult> GetActive()
    {
        var promoCodes = await _promoCodeService.GetActivePromoCodesAsync();
        return Ok(promoCodes);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var promoCode = await _promoCodeService.GetPromoCodeByIdAsync(id);
        if (promoCode is null)
        {
            return NotFound();
        }

        return Ok(promoCode);
    }

    [HttpGet("code/{code}")]
    public async Task<IActionResult> GetByCode(string code)
    {
        var promoCode = await _promoCodeService.GetPromoCodeByCodeAsync(code);
        if (promoCode is null)
        {
            return NotFound();
        }

        return Ok(promoCode);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePromoCodeDto createPromoCodeDtodto)
    {
        var result = await _promoCodeService.CreatePromoCodeAsync(createPromoCodeDtodto);
        
        if (!result.IsSuccess)
            return StatusCode(result.ErrorCode, new { message = result.Message });
        
        return CreatedAtAction(nameof(GetById), new { id = result.Value!.PromoCodeId }, result.Value);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePromoCodeDto updatePromoCodeDto)
    {
        var result = await _promoCodeService.UpdatePromoCodeAsync(id, updatePromoCodeDto);
        
        if (!result.IsSuccess)
            return StatusCode(result.ErrorCode, new { message = result.Message });
        
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _promoCodeService.DeletePromoCodeAsync(id);
        
        if (!result.IsSuccess)
            return StatusCode(result.ErrorCode, new { message = result.Message });
        
        return NoContent();
    }

    [HttpPost("validate")]
    [AllowAnonymous]
    public async Task<IActionResult> Validate([FromBody] PromoCodeValidationResults validationResults)
    {
        var result = await _promoCodeService.ValidatePromoCodeAsync(validationResults);

        if (!result.IsValid)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
    
    [HttpPatch("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id)
    {
        var result = await _promoCodeService.ActivatePromoCodeAsync(id);
        if (!result.IsSuccess)
            return StatusCode(result.ErrorCode, new { message = result.Message });
        return NoContent();
    }

    [HttpPatch("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var result = await _promoCodeService.DeactivatePromoCodeAsync(id);
        if (!result.IsSuccess)
            return StatusCode(result.ErrorCode, new { message = result.Message });
        return NoContent();
    }
}