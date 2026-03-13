using PharmacyApp.Application.DTOs.ShoppingCart;

namespace PharmacyApp.Application.Interfaces.Services;

public interface IShoppingCartService
{
    Task<CartDto?> GetCartAsync(string? userId, string? sessionId);
    Task<CartDto> AddToCartAsync(string? userId, string? sessionId, AddToCartDto addToCartDto);
    Task<CartDto?> UpdateCartItemAsync(string? userId, string? sessionId, UpdateCartDto updateCartDto);
    Task RemoveCartItemAsync(string? userId, string? sessionId, int productId);
    Task ClearCartAsync(string? userId, string? sessionId);
    Task MergeCartsOnLoginAsync(string sessionId, string userId);
}
