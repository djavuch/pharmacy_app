using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyApp.Application.DTOs.PromoCode;
using PharmacyApp.Application.Interfaces.Services;

namespace PharmacyApp.Presentation.Controllers.Admin;

[ApiController]
[Route("admin/promocodes")]
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
    public async Task<IActionResult> Create([FromBody] CreatePromoCodeDto dto)
    {
        var created = await _promoCodeService.CreatePromoCodeAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.PromoCodeId }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePromoCodeDto dto)
    {
        await _promoCodeService.UpdatePromoCodeAsync(id, dto);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _promoCodeService.DeletePromoCodeAsync(id);
        return NoContent();
    }

    [HttpPost("validate")]
    [AllowAnonymous]
    public async Task<IActionResult> Validate([FromBody] ValidatePromoCodeDto dto)
    {
        var result = await _promoCodeService.ValidatePromoCodeAsync(dto);

        if (!result.IsValid)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}