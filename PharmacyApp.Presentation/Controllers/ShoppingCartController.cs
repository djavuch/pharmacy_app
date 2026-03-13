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
    private readonly ILogger<ShoppingCartController> _logger;

    public ShoppingCartController(IShoppingCartService shoppingCartService, ILogger<ShoppingCartController> logger)
    {
        _shoppingCartService = shoppingCartService;
        _logger = logger;
    }

    private (string? userId, string? sessionId) GetUserIdentifiers()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var sessionId = SessionHelper.GetSessionId(HttpContext);

        _logger.LogInformation("GetUserIdentifiers - UserId: {UserId}, SessionId: {SessionId}, IsAuthenticated: {IsAuth}",
            userId, sessionId, User.Identity?.IsAuthenticated);

        return (userId, sessionId);
    }

    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var (userId, sessionId) = GetUserIdentifiers();
        var cart = await _shoppingCartService.GetCartAsync(userId, sessionId);
        return Ok(cart);
    }

    [HttpPost]
    public async Task<IActionResult> AddToCart(AddToCartDto addToCartDto)
    {
        var (userId, sessionId) = GetUserIdentifiers();
        var result = await _shoppingCartService.AddToCartAsync(userId, sessionId, addToCartDto);
        return Ok(result);
    }

    [HttpPut("items/{productId}")]
    public async Task<IActionResult> UpdateQuantity(UpdateCartDto updateCartDto)
    {
        var (userId, sessionId) = GetUserIdentifiers();
        var result = await _shoppingCartService.UpdateCartItemAsync(userId, sessionId, updateCartDto);
        return Ok(result);
    }

    [HttpDelete("items/{productId}")]
    public async Task<IActionResult> RemoveItem(int productId)
    {
        var (userId, sessionId) = GetUserIdentifiers();
        await _shoppingCartService.RemoveCartItemAsync(userId, sessionId, productId);
        return NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> ClearCart()
    {
        var (userId, sessionId) = GetUserIdentifiers();
        await _shoppingCartService.ClearCartAsync(userId, sessionId);
        return NoContent();
    }
}
