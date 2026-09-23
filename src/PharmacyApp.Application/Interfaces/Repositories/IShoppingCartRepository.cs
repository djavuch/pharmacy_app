using PharmacyApp.Domain.Entities;

namespace PharmacyApp.Application.Interfaces.Repositories;

public interface IShoppingCartRepository
{
    Task<ShoppingCart?> GetByUserIdAsync(string userId, CancellationToken ct = default);
    Task<ShoppingCart?> GetBySessionIdAsync(string sessionId, CancellationToken ct = default);
    Task<ShoppingCart?> GetByUserOrSessionAsync(string? userId, string? sessionId, CancellationToken ct = default);
    Task<CartItem?> GetItemAsync(int cartId, int productId);
    Task<ShoppingCart> AddAsync(ShoppingCart cart, CancellationToken ct = default);
    Task UpdateAsync(ShoppingCart cart);
    Task AddItemAsync(CartItem cartItem, CancellationToken ct = default);
    Task UpdateItemAsync(CartItem cartItem);
    Task RemoveItemAsync(CartItem cartItem);
    Task ClearAsync(int cartId, CancellationToken ct = default);
    Task MigrateCartAsync(string sessionId, string userId, CancellationToken ct = default);
}
