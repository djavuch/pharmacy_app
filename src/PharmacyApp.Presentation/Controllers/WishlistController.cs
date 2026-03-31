using Microsoft.AspNetCore.Mvc;
using PharmacyApp.Application.DTOs.Wishlist;
using PharmacyApp.Application.Interfaces.Services;
using System.Security.Claims;
using static PharmacyApp.Domain.Exceptions.AppExceptions;

namespace PharmacyApp.Presentation.Controllers;

[ApiController]
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

        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        var result = await _wishlistService.AddToWishlistAsync(wishlistAddDto, userId);
        return Ok(result);
    }

    [HttpDelete("{productId}")]
    public async Task<IActionResult> RemoveFromWishlist(int productId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        await _wishlistService.RemoveFromWishlistAsync(userId, productId);

        return Ok();
    }
}
