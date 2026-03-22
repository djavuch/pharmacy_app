using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyApp.Application.DTOs.Discount;
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

    /// <summary>Gets all discounts.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var discounts = await _discountService.GetAllDiscountsAsync();
        return Ok(discounts);
    }

    /// <summary>Gets only currently active discounts.</summary>
    [HttpGet("active")]
    [AllowAnonymous]
    public async Task<IActionResult> GetActive()
    {
        var discounts = await _discountService.GetActiveDiscountsAsync();
        return Ok(discounts);
    }

    /// <summary>Gets a single discount by ID.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var discount = await _discountService.GetDiscountByIdAsync(id);
        if (discount is null)
            return NotFound();

        return Ok(discount);
    }

    /// <summary>Creates a new discount, optionally assigned to products/categories.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDiscountDto dto)
    {
        var created = await _discountService.CreateDiscountAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.DiscountId }, created);
    }

    /// <summary>Updates an existing discount and replaces its product/category assignments.</summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDiscountDto dto)
    {
        await _discountService.UpdateDiscountAsync(id, dto);
        return NoContent();
    }

    /// <summary>Deletes a discount.</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _discountService.DeleteDiscountAsync(id);
        return NoContent();
    }

    /// <summary>Calculates the discounted price for a product given its original price.</summary>
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