﻿using Microsoft.EntityFrameworkCore;
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

    public async Task<ShoppingCart?> GetByUserIdAsync(string userId)
    {
        return await _dbContext.ShoppingCart
              .Include(sc => sc.Items)
                .ThenInclude(i => i.Product)
              .FirstOrDefaultAsync(sc => sc.UserId == userId);
    }

    public async Task<ShoppingCart?> GetBySessionIdAsync(string sessionId)
    {
        return await _dbContext.ShoppingCart
              .Include(sc => sc.Items)
                .ThenInclude(i => i.Product)
              .FirstOrDefaultAsync(sc => sc.SessionId == sessionId);
    }

    public async Task<ShoppingCart?> GetByUserOrSessionAsync(string? userId, string? sessionId)
    {
        if (!string.IsNullOrEmpty(userId))
        {
            return await GetByUserIdAsync(userId);
        }

        if (!string.IsNullOrEmpty(sessionId))
            return await GetBySessionIdAsync(sessionId);

        return null;
    }

    public async Task<CartItem?> GetItemAsync(int cartId, int productId)
    {
        return await _dbContext.CartItems
            .FirstOrDefaultAsync(ci => ci.CartId == cartId && ci.ProductId == productId);
    }

    public async Task<ShoppingCart> AddAsync(ShoppingCart cart)
    {
        await _dbContext.ShoppingCart.AddAsync(cart);
        return cart;
    }

    public Task UpdateAsync(ShoppingCart cart)
    {
        _dbContext.ShoppingCart.Update(cart);
        return Task.CompletedTask;
    }

    public async Task AddItemAsync(CartItem cartItem)
    {
        await _dbContext.CartItems.AddAsync(cartItem);
    }

    public Task UpdateItemAsync(CartItem cartItem)
    {
        _dbContext.CartItems.Update(cartItem);
        return Task.CompletedTask;
    }

    public Task RemoveItemAsync(CartItem cartItem)
    {
        _dbContext.CartItems.Remove(cartItem);
        return Task.CompletedTask;
    }

    public Task ClearAsync(int cartId)
    {
        return _dbContext.CartItems
            .Where(ci => ci.CartId == cartId)
            .ExecuteDeleteAsync();
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

        if (!string.IsNullOrWhiteSpace(sessionCart.UserId))
        {
            if (!string.Equals(sessionCart.UserId, userId, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning(
                    "Skipping migration: session cart {CartId} already belongs to another user {SessionUserId}, target user {TargetUserId}",
                    sessionCart.Id,
                    sessionCart.UserId,
                    userId);
                return;
            }

            sessionCart.AssignToUser(userId);
            _dbContext.ShoppingCart.Update(sessionCart);

            _logger.LogInformation(
                "Session cart {CartId} already belongs to user {UserId}. Detached from session and completed.",
                sessionCart.Id,
                userId);
            return;
        }

        var userCart = await GetByUserIdAsync(userId);

        if (userCart is not null && userCart.Id == sessionCart.Id)
        {
            userCart.AssignToUser(userId);
            _dbContext.ShoppingCart.Update(userCart);

            _logger.LogInformation(
                "Session cart and user cart are the same entity (CartId: {CartId}). Detached from session and completed.",
                userCart.Id);
            return;
        }

        if (userCart == null)
        {
            _logger.LogInformation("No existing user cart, converting session cart to user cart - UserId: {UserId}",
                userId);

            sessionCart.AssignToUser(userId);
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

                if (existingItem is not null)
                {
                    _logger.LogInformation("Updating existing item - ProductId: {ProductId}, OldQty: {OldQty}, AddingQty: {AddingQty}",
                        item.ProductId, existingItem.Quantity, item.Quantity);

                    existingItem.AddQuantity(item.Quantity);
                    _dbContext.CartItems.Update(existingItem);
                    updatedItems++;
                }
                else
                {
                    _logger.LogInformation("Adding new item to user cart - ProductId: {ProductId}, Qty: {Quantity}",
                        item.ProductId, item.Quantity);

                    var newItem = new CartItem(userCart.Id, item.ProductId, item.Quantity, item.PriceAtAdd);
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
