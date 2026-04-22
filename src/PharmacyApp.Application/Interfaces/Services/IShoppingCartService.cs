using PharmacyApp.Application.Contracts.ShoppingCart;
using PharmacyApp.Domain.Common;

namespace PharmacyApp.Application.Interfaces.Services;

public interface IShoppingCartService
{
    Task<Result<CartDto>> GetCartAsync(string? userId, string? sessionId);
    Task<Result<CartDto>> AddToCartAsync(string? userId, string? sessionId, AddToCartDto addToCartDto);
    Task<Result<CartDto>> UpdateCartItemAsync(string? userId, string? sessionId, UpdateCartDto updateCartDto);
    Task<Result> RemoveCartItemAsync(string? userId, string? sessionId, int productId);
    Task<Result> ClearCartAsync(string? userId, string? sessionId);
    Task MergeCartsOnLoginAsync(string sessionId, string userId, bool replaceExistingItems = false);
    Task<Result> MergeCartsOnLogoutAsync(string userId, string sessionId);
    Task ClearCartByUserIdAsync(string userId);
}
