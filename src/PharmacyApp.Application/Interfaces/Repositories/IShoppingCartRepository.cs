using PharmacyApp.Domain.Entities;

namespace PharmacyApp.Application.Interfaces.Repositories;

public interface IShoppingCartRepository
{
    Task<ShoppingCart?> GetByUserIdAsync(string userId);
    Task<ShoppingCart?> GetBySessionIdAsync(string sessionId);
    Task<ShoppingCart?> GetByUserOrSessionAsync(string? userId, string? sessionId);
    Task<CartItem?> GetItemAsync(int cartId, int productId);
    Task<ShoppingCart> AddAsync(ShoppingCart cart);
    Task UpdateAsync(ShoppingCart cart);
    Task AddItemAsync(CartItem cartItem);
    Task UpdateItemAsync(CartItem cartItem);
    Task RemoveItemAsync(int cartId, int productId);
    Task ClearAsync(int cartId);
    Task MigrateCartAsync(string sessionId, string userId);
}
