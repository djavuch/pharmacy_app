using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using PharmacyApp.Application.Interfaces.Services;
using System.Security.Claims;
using PharmacyApp.Application.Contracts.Wishlist;
using static PharmacyApp.Domain.Exceptions.AppExceptions;

namespace PharmacyApp.Presentation.Controllers;

[ApiController]
[EnableCors("AllowFrontend")]
[Route("user/wishlist")]
public class WishlistController : ControllerBase
{
    private readonly IWishlistService _wishlistService;
    public WishlistController(IWishlistService wishlistService)
    {
        _wishlistService = wishlistService;
    }

    [HttpGet]
    public async Task<IActionResult> GetWishlist()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        var wishlist = await _wishlistService.GetWishlistByUserIdAsync(userId);
        return Ok(wishlist);
    }

    [HttpPost]
    public async Task<IActionResult> AddToWishlist(WishlistDto wishlistAddDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _wishlistService.AddToWishlistAsync(wishlistAddDto, userId);
        
        if (!result.IsSuccess)
            return StatusCode(result.ErrorType.ToStatusCode(), new { message = result.Message });
        
        return Ok(result.Value);
    }

    [HttpDelete("{productId:int}")]
    public async Task<IActionResult> RemoveFromWishlist(int productId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _wishlistService.RemoveFromWishlistAsync(userId, productId);
        
        if (!result.IsSuccess)
            return StatusCode(result.ErrorType.ToStatusCode(), new { message = result.Message });
        
        return NoContent();
    }
}
