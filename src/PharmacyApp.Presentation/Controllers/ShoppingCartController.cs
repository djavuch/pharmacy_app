using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PharmacyApp.Application.DTOs.ShoppingCart;
using PharmacyApp.Application.Interfaces.Services;
using PharmacyApp.Presentation.Helpers;
using System.Security.Claims;

namespace PharmacyApp.Presentation.Controllers;

[ApiController]
[Route("cart")]
public class ShoppingCartController : ControllerBase
{
    private readonly IShoppingCartService _shoppingCartService;

    public ShoppingCartController(IShoppingCartService shoppingCartService)
    {
        _shoppingCartService = shoppingCartService;
    }

    private (string? userId, string? sessionId) GetUserIdentifiers()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrWhiteSpace(userId))
        {
            var existingSessionId = SessionHelper.TryGetSessionId(HttpContext);
            if (!string.IsNullOrEmpty(existingSessionId))
            {
                return (userId, existingSessionId);
            }
            return (userId, null);
        }
        
        var sessionId = SessionHelper.GetOrCreateSessionId(HttpContext);

        return (null, sessionId);
    }

    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var (userId, sessionId) = GetUserIdentifiers();
        var cart = await _shoppingCartService.GetCartAsync(userId, sessionId);
        
        if (!string.IsNullOrWhiteSpace(userId) && !string.IsNullOrEmpty(sessionId))
        {
            SessionHelper.ClearSessionId(HttpContext);
        }
        
        return Ok(cart);
    }

    [HttpPost]
    public async Task<IActionResult> AddToCart(AddToCartDto addToCartDto)
    {
        var (userId, sessionId) = GetUserIdentifiers();
        var result = await _shoppingCartService.AddToCartAsync(userId, sessionId, addToCartDto);
        
        if (!string.IsNullOrWhiteSpace(userId) && !string.IsNullOrEmpty(sessionId))
        {
            SessionHelper.ClearSessionId(HttpContext);
        }
        
        return Ok(result);
    }

    [HttpPut("items/{productId}")]
    public async Task<IActionResult> UpdateQuantity(UpdateCartDto updateCartDto)
    {
        var (userId, sessionId) = GetUserIdentifiers();
        var result = await _shoppingCartService.UpdateCartItemAsync(userId, sessionId, updateCartDto);
        
        if (!string.IsNullOrWhiteSpace(userId) && !string.IsNullOrEmpty(sessionId))
        {
            SessionHelper.ClearSessionId(HttpContext);
        }
        
        return Ok(result);
    }

    [HttpDelete("items/{productId}")]
    public async Task<IActionResult> RemoveItem(int productId)
    {
        var (userId, sessionId) = GetUserIdentifiers();
        await _shoppingCartService.RemoveCartItemAsync(userId, sessionId, productId);
        
        if (!string.IsNullOrWhiteSpace(userId) && !string.IsNullOrEmpty(sessionId))
        {
            SessionHelper.ClearSessionId(HttpContext);
        }
        
        return NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> ClearCart()
    {
        var (userId, sessionId) = GetUserIdentifiers();
        await _shoppingCartService.ClearCartAsync(userId, sessionId);
        
        
        if (!string.IsNullOrWhiteSpace(userId) && !string.IsNullOrEmpty(sessionId))
        {
            SessionHelper.ClearSessionId(HttpContext);
        }
        
        return NoContent();
    }
}
