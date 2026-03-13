using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PharmacyApp.Application.Interfaces.Repositories;
using PharmacyApp.Domain.Entities;
using PharmacyApp.Infrastructure.Data;

namespace PharmacyApp.Infrastructure.Repositories;

public class ShoppingCartRepository : IShoppingCartRepository
{
    private readonly PharmacyAppDbContext _dbContext;
    private readonly ILogger<ShoppingCartRepository> _logger;

    public ShoppingCartRepository(PharmacyAppDbContext dbContext, ILogger<ShoppingCartRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<ShoppingCartModel?> GetByUserIdAsync(string userId)
    {
        return await _dbContext.ShoppingCart
              .Include(sc => sc.Items)
                .ThenInclude(i => i.Product)
              .FirstOrDefaultAsync(sc => sc.UserId == userId);
    }

    public async Task<ShoppingCartModel?> GetBySessionIdAsync(string sessionId)
    {
        return await _dbContext.ShoppingCart
              .Include(sc => sc.Items)
                .ThenInclude(i => i.Product)
              .FirstOrDefaultAsync(sc => sc.SessionId == sessionId);
    }

    public async Task<ShoppingCartModel?> GetByUserOrSessionAsync(string? userId, string? sessionId)
    {
        if (!string.IsNullOrEmpty(userId))
        {
            var userCart = await GetByUserIdAsync(userId);
            if (userCart != null)
            {
                return userCart;
            }
        }

        // Если корзина пользователя не найдена, ищем по SessionId
        if (!string.IsNullOrEmpty(sessionId))
        {
            return await GetBySessionIdAsync(sessionId);
        }

        return null;
    }

    public async Task<CartItemModel?> GetItemAsync(int cartId, int productId)
    {
        return await _dbContext.CartItems
            .FirstOrDefaultAsync(ci => ci.CartId == cartId && ci.ProductId == productId);
    }

    public async Task<ShoppingCartModel> AddAsync(ShoppingCartModel cart)
    {
        await _dbContext.ShoppingCart.AddAsync(cart);
        return cart;
    }

    public Task UpdateAsync(ShoppingCartModel cart)
    {
        _dbContext.ShoppingCart.Update(cart);
        return Task.CompletedTask;
    }

    public async Task AddItemAsync(CartItemModel cartItem)
    {
        await _dbContext.CartItems.AddAsync(cartItem);
    }

    public Task UpdateItemAsync(CartItemModel cartItem)
    {
        _dbContext.CartItems.Update(cartItem);
        return Task.CompletedTask;
    }

    public async Task RemoveItemAsync(int cartId, int productId)
    {
        var cartItem = await GetItemAsync(cartId, productId);
        if (cartItem != null)
        {
            _dbContext.CartItems.Remove(cartItem);
        }
    }

    public async Task ClearAsync(int cartId)
    {
        var items = await _dbContext.CartItems
            .Where(ci => ci.CartId == cartId)
            .ToListAsync();
        _dbContext.CartItems.RemoveRange(items);
    }

    public async Task MigrateCartAsync(string sessionId, string userId)
    {
        _logger.LogInformation("Starting cart migration - SessionId: {SessionId}, UserId: {UserId}",
            sessionId, userId);

        var sessionCart = await GetBySessionIdAsync(sessionId);

        if (sessionCart == null)
        {
            _logger.LogWarning("Session cart not found - SessionId: {SessionId}", sessionId);
            return;
        }

        _logger.LogInformation("Session cart found - CartId: {CartId}, Items count: {ItemsCount}",
            sessionCart.Id, sessionCart.Items.Count);

        var userCart = await GetByUserIdAsync(userId);

        if (userCart == null)
        {
            _logger.LogInformation("No existing user cart, converting session cart to user cart - UserId: {UserId}",
                userId);

            sessionCart.UserId = userId;
            sessionCart.SessionId = null;
            _dbContext.ShoppingCart.Update(sessionCart);

            _logger.LogInformation("Session cart converted to user cart - CartId: {CartId}, UserId: {UserId}",
                sessionCart.Id, userId);
        }
        else
        {
            _logger.LogInformation("User cart exists - CartId: {CartId}, Current items: {ItemsCount}",
                userCart.Id, userCart.Items.Count);

            int mergedItems = 0;
            int updatedItems = 0;

            foreach (var item in sessionCart.Items.ToList())
            {
                var existingItem = await GetItemAsync(userCart.Id, item.ProductId);

                if (existingItem != null)
                {
                    _logger.LogInformation("Updating existing item - ProductId: {ProductId}, OldQty: {OldQty}, AddingQty: {AddingQty}",
                        item.ProductId, existingItem.Quantity, item.Quantity);

                    existingItem.Quantity += item.Quantity;
                    _dbContext.CartItems.Update(existingItem);
                    updatedItems++;
                }
                else
                {
                    _logger.LogInformation("Adding new item to user cart - ProductId: {ProductId}, Qty: {Quantity}",
                        item.ProductId, item.Quantity);

                    var newItem = new CartItemModel
                    {
                        CartId = userCart.Id,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        PriceAtAdd = item.PriceAtAdd
                    };
                    await _dbContext.CartItems.AddAsync(newItem);
                    mergedItems++;
                }
            }

            _logger.LogInformation("Removing session cart - CartId: {CartId}", sessionCart.Id);
            _dbContext.ShoppingCart.Remove(sessionCart);

            userCart.UpdateTimestamp();
            _dbContext.ShoppingCart.Update(userCart);

            _logger.LogInformation("Cart migration completed - Merged: {MergedItems}, Updated: {UpdatedItems}, UserCartId: {UserCartId}",
                mergedItems, updatedItems, userCart.Id);
        }
    }
}