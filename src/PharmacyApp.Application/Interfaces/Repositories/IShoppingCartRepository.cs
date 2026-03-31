using PharmacyApp.Domain.Entities;

namespace PharmacyApp.Application.Interfaces.Repositories;

public interface IShoppingCartRepository
{
    Task<ShoppingCartModel?> GetByUserIdAsync(string userId);
    Task<ShoppingCartModel?> GetBySessionIdAsync(string sessionId);
    Task<ShoppingCartModel?> GetByUserOrSessionAsync(string? userId, string? sessionId);
    Task<CartItemModel?> GetItemAsync(int cartId, int productId);
    Task<ShoppingCartModel> AddAsync(ShoppingCartModel cart);
    Task UpdateAsync(ShoppingCartModel cart);
    Task AddItemAsync(CartItemModel cartItem);
    Task UpdateItemAsync(CartItemModel cartItem);
    Task RemoveItemAsync(int cartId, int productId);
    Task ClearAsync(int cartId);
    Task MigrateCartAsync(string sessionId, string userId);
}
