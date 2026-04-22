using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyApp.Application.Contracts.Discount;
using PharmacyApp.Application.Interfaces.Services;

namespace PharmacyApp.Presentation.Controllers.Admin;

[ApiController]
[Route("admin/discounts")]
[Authorize(Roles = "Admin")]
public class DiscountController : ControllerBase
{
    private readonly IDiscountService _discountService;

    public DiscountController(IDiscountService discountService)
    {
        _discountService = discountService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var discounts = await _discountService.GetAllDiscountsAsync();
        return Ok(discounts);
    }
    
    [HttpGet("active")]
    [AllowAnonymous]
    public async Task<IActionResult> GetActive()
    {
        var discounts = await _discountService.GetActiveDiscountsAsync();
        return Ok(discounts);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var discount = await _discountService.GetDiscountByIdAsync(id);
        if (discount is null)
            return NotFound();

        return Ok(discount);
    }
    
    [HttpPost]
    public async Task<IActionResult> Create(CreateDiscountDto сreateDiscountDto)
    {
        var result = await _discountService.CreateDiscountAsync(сreateDiscountDto);
        
        if (!result.IsSuccess)
            return StatusCode(result.ErrorType.ToStatusCode(), new { message = result.Message });
        
        return CreatedAtAction(nameof(GetById), new { id = result.Value!.DiscountId }, result.Value);
    }
    
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateDiscountDto updateDiscountDto)
    {
        var result = await _discountService.UpdateDiscountAsync(id, updateDiscountDto);
        
        if (!result.IsSuccess)
            return StatusCode(result.ErrorType.ToStatusCode(), new { message = result.Message });
        
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _discountService.DeleteDiscountAsync(id);
        
        if (!result.IsSuccess)
            return StatusCode(result.ErrorType.ToStatusCode(), new { message = result.Message });
        
        return NoContent();
    }

    [HttpGet("product/{productId:int}/price")]
    [AllowAnonymous]
    public async Task<IActionResult> CalculatePrice(int productId, int categoryId, decimal price)
    {
        if (price <= 0)
            return BadRequest("Price must be greater than 0.");

        var discountedPrice = await _discountService.CalculateDiscountedPriceAsync(productId, categoryId, price);
        return Ok(new { originalPrice = price, discountedPrice });
    }
}
