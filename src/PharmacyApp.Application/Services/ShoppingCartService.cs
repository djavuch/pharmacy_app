using Microsoft.Extensions.Logging;
using PharmacyApp.Application.Contracts.ShoppingCart;
using PharmacyApp.Application.Interfaces.Repositories;
using PharmacyApp.Application.Interfaces.Services;
using PharmacyApp.Application.Mappers;
using PharmacyApp.Domain.Common;
using PharmacyApp.Domain.Entities;

namespace PharmacyApp.Application.Services;

public class ShoppingCartService : IShoppingCartService
{
    private readonly IUnitOfWorkRepository _unitOfWork;
    private readonly ILogger<ShoppingCartService> _logger;  

    public ShoppingCartService(IUnitOfWorkRepository unitOfWork, ILogger<ShoppingCartService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    private async Task<ShoppingCart> GetOrCreateCartAsync(string? userId, string? sessionId)
    {
        _logger.LogInformation("GetOrCreateCart - UserId: {UserId}, SessionId: {SessionId}",
            userId ?? "null", sessionId ?? "null");

        var cart = await _unitOfWork.ShoppingCarts.GetByUserOrSessionAsync(userId, sessionId);

        if (cart is null)
        {
            _logger.LogInformation("Creating new cart - UserId: {UserId}, SessionId: {SessionId}",
                userId ?? "null", sessionId ?? "null");

            var isAuthenticatedUser = !string.IsNullOrWhiteSpace(userId);
            cart = new ShoppingCart(userId, isAuthenticatedUser ? null : sessionId);
            
            await _unitOfWork.ShoppingCarts.AddAsync(cart);
            await _unitOfWork.SaveChangesAsync();
        }
        else
        {
            _logger.LogInformation("Found existing cart - CartId: {CartId}, UserId: {UserId}, SessionId: {SessionId}, Items: {ItemsCount}",
                cart.Id, cart.UserId ?? "null", cart.SessionId ?? "null", cart.Items.Count);
        }

        return cart;
    }

    public async Task<Result<CartDto>> GetCartAsync(string? userId, string? sessionId)
    {
        if (string.IsNullOrEmpty(userId) && string.IsNullOrEmpty(sessionId))
            return Result<CartDto>.Conflict("Either userId or sessionId must be provided.");

        var cart = await GetOrCreateCartAsync(userId, sessionId);

        return Result<CartDto>.Success(cart.ToCartDto());
    }

    public async Task<Result<CartDto>> AddToCartAsync(string? userId, string? sessionId, AddToCartDto addToCartDto)
    {
        if (string.IsNullOrEmpty(userId) && string.IsNullOrEmpty(sessionId))
            return Result<CartDto>.Conflict("Either userId or sessionId must be provided.");
        
        _logger.LogInformation("AddToCart - ProductId: {ProductId}, Quantity: {Quantity}, UserId: {UserId}, SessionId: {SessionId}",
            addToCartDto.ProductId, addToCartDto.Quantity, userId ?? "null", sessionId ?? "null");

        var product = await _unitOfWork.Products.GetByIdAsync(addToCartDto.ProductId);

        if (product is null)
            return Result<CartDto>.NotFound("Product not found.");

        if (product.StockQuantity < addToCartDto.Quantity)
            return Result<CartDto>.Conflict("Insufficient stock for the requested product.");
        
        var cart = await GetOrCreateCartAsync(userId, sessionId);

        var existingItem = await _unitOfWork.ShoppingCarts.GetItemAsync(cart.Id, addToCartDto.ProductId);

        if (existingItem is not null)
        {
            existingItem.AddQuantity(addToCartDto.Quantity);

            if (existingItem.Quantity > product.StockQuantity)
                return Result<CartDto>.Conflict($"Total quantity exceeds available stock: {product.StockQuantity}");
            
            await _unitOfWork.ShoppingCarts.UpdateItemAsync(existingItem);
        }
        else
        {
            var newItem = new CartItem(cart.Id, addToCartDto.ProductId, addToCartDto.Quantity, product.Price);

            await _unitOfWork.ShoppingCarts.AddItemAsync(newItem);
        }

        cart.UpdateTimestamp();
        await _unitOfWork.ShoppingCarts.UpdateAsync(cart);
        await _unitOfWork.SaveChangesAsync();

        return await GetCartAsync(userId, sessionId);
    }

    public async Task<Result<CartDto>> UpdateCartItemAsync(string? userId, string? sessionId, UpdateCartDto updateCartDto)
    {
        var cart = await _unitOfWork.ShoppingCarts.GetByUserOrSessionAsync(userId, sessionId);

        if (cart is null)
            return Result<CartDto>.NotFound("Shopping cart not found.");
        
        var cartItem = await _unitOfWork.ShoppingCarts.GetItemAsync(cart.Id, updateCartDto.ProductId);

        if (cartItem is null)
            return Result<CartDto>.NotFound($"Product with ID {updateCartDto.ProductId} not found in cart.");
        
        if (updateCartDto.Quantity <= 0)
        {
            await _unitOfWork.ShoppingCarts.RemoveItemAsync(cart.Id, updateCartDto.ProductId);
            cart.UpdateTimestamp();
            await _unitOfWork.ShoppingCarts.UpdateAsync(cart);
            await _unitOfWork.SaveChangesAsync();
            return await GetCartAsync(userId, sessionId);
        }

        var product = await _unitOfWork.Products.GetByIdAsync(updateCartDto.ProductId);

        if (product is null)
            return Result<CartDto>.NotFound($"Product with ID {updateCartDto.ProductId} not found");

        if (updateCartDto.Quantity > product.StockQuantity)
            return Result<CartDto>.Conflict($"Requested quantity exceeds available stock: {product.StockQuantity}");
        
        cartItem.SetQuantity(updateCartDto.Quantity);
        await _unitOfWork.ShoppingCarts.UpdateItemAsync(cartItem);

        cart.UpdateTimestamp();
        await _unitOfWork.ShoppingCarts.UpdateAsync(cart);
        await _unitOfWork.SaveChangesAsync();

        return await GetCartAsync(userId, sessionId);
    }

    public async Task<Result> RemoveCartItemAsync(string? userId, string? sessionId, int productId)
    {
        var cart = await _unitOfWork.ShoppingCarts.GetByUserOrSessionAsync(userId, sessionId);

        if (cart is null)
            return Result.NotFound("Shopping cart not found.");
        
        await _unitOfWork.ShoppingCarts.RemoveItemAsync(cart.Id, productId);
        cart.UpdateTimestamp();

        await _unitOfWork.ShoppingCarts.UpdateAsync(cart);
        await _unitOfWork.SaveChangesAsync();
        
        return  Result.Success();
    }

    public async Task<Result> ClearCartAsync(string? userId, string? sessionId)
    {
        var cart = await _unitOfWork.ShoppingCarts.GetByUserOrSessionAsync(userId, sessionId);

        if (cart is null)
            return Result.NotFound("Shopping cart not found.");

        await _unitOfWork.ShoppingCarts.ClearAsync(cart.Id);

        cart.UpdateTimestamp();
        await _unitOfWork.ShoppingCarts.UpdateAsync(cart);
        await _unitOfWork.SaveChangesAsync();
        
        return Result.Success();
    }

    public async Task MergeCartsOnLoginAsync(string sessionId, string userId, bool replaceExistingItems = false)
    {
        _logger.LogInformation("========== MERGE CARTS START ==========");
        _logger.LogInformation("MERGE: SessionId: {SessionId}", sessionId);
        _logger.LogInformation("MERGE: UserId: {UserId}", userId);
        _logger.LogInformation("MERGE: ReplaceExistingItems: {ReplaceExistingItems}", replaceExistingItems);
        
        var sessionCart = await _unitOfWork.ShoppingCarts.GetBySessionIdAsync(sessionId);
        if (sessionCart is not null)
        {
            _logger.LogInformation("MERGE: Found session cart - CartId: {CartId}, Items: {ItemsCount}, UserId: {UserId}, SessionId: {SessionId}",
                sessionCart.Id, sessionCart.Items.Count, sessionCart.UserId ?? "null", sessionCart.SessionId ?? "null");
        }
        else
        {
            _logger.LogWarning("MERGE: No session cart found for SessionId: {SessionId}", sessionId);
        }
        
        var userCart = await _unitOfWork.ShoppingCarts.GetByUserIdAsync(userId);
        if (userCart is not null)
        {
            _logger.LogInformation("MERGE: Found existing user cart - CartId: {CartId}, Items: {ItemsCount}",
                userCart.Id, userCart.Items.Count);
        }
        else
        {
            _logger.LogInformation("MERGE: No existing user cart found");
        }

        if (replaceExistingItems &&
            sessionCart is not null &&
            userCart is not null &&
            userCart.Id != sessionCart.Id)
        {
            _logger.LogInformation(
                "MERGE: Clearing existing user cart before replacement merge - CartId: {CartId}",
                userCart.Id);

            await _unitOfWork.ShoppingCarts.ClearAsync(userCart.Id);
            userCart.UpdateTimestamp();
            await _unitOfWork.ShoppingCarts.UpdateAsync(userCart);
        }

        await _unitOfWork.ShoppingCarts.MigrateCartAsync(sessionId, userId);
        await _unitOfWork.SaveChangesAsync();
        
        var mergedCart = await _unitOfWork.ShoppingCarts.GetByUserIdAsync(userId);
        if (mergedCart != null)
        {
            _logger.LogInformation("MERGE RESULT: User cart now has {ItemsCount} items", mergedCart.Items.Count);
        }
        else
        {
            _logger.LogError("MERGE RESULT: No user cart found after migration!");
        }

        _logger.LogInformation("========== MERGE CARTS COMPLETED ==========");
    }
    
    public async Task<Result> MergeCartsOnLogoutAsync(string userId, string sessionId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return Result.NotFound("userId is required.");
    
        if (string.IsNullOrWhiteSpace(sessionId))
            return Result.Conflict("sessionId is required.");
    
        _logger.LogInformation("========== COPY USER CART TO SESSION START ==========");
        _logger.LogInformation("COPY: UserId: {UserId}, SessionId: {SessionId}", userId, sessionId);
    
        var userCart = await _unitOfWork.ShoppingCarts.GetByUserIdAsync(userId);
        if (userCart is null)
        {
            _logger.LogInformation("COPY: User cart not found. Nothing to copy.");
            return Result.NotFound("User cart not found.");
        }
        
        var sessionCart = await _unitOfWork.ShoppingCarts.GetBySessionIdAsync(sessionId);

        if (sessionCart is not null && !string.IsNullOrWhiteSpace(sessionCart.UserId))
        {
            if (!string.Equals(sessionCart.UserId, userId, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning(
                    "COPY: Session cart {CartId} belongs to another user {SessionUserId}. Copy aborted for {UserId}.",
                    sessionCart.Id,
                    sessionCart.UserId,
                    userId);
                return Result.Conflict("Session cart belongs to another user.");
            }

            sessionCart.AssignToUser(userId);
            await _unitOfWork.ShoppingCarts.UpdateAsync(sessionCart);
            await _unitOfWork.SaveChangesAsync();
            sessionCart = null;
        }

        if (sessionCart is null)
        {
            sessionCart = new ShoppingCart(null!, sessionId);
            await _unitOfWork.ShoppingCarts.AddAsync(sessionCart);
            await _unitOfWork.SaveChangesAsync();
        }

        await _unitOfWork.ShoppingCarts.ClearAsync(sessionCart.Id);
    
        foreach (var item in userCart.Items)
        {
            await _unitOfWork.ShoppingCarts.AddItemAsync(new CartItem(
                sessionCart.Id,
                item.ProductId,
                item.Quantity,
                item.PriceAtAdd));
        }
    
        sessionCart.UpdateTimestamp();
        await _unitOfWork.ShoppingCarts.UpdateAsync(sessionCart);
        await _unitOfWork.SaveChangesAsync();
        
        _logger.LogInformation("========== COPY USER CART TO SESSION COMPLETED ==========");
        return Result.Success();
    }

    public async Task ClearCartByUserIdAsync(string userId)
    {
        var cart = await _unitOfWork.ShoppingCarts.GetByUserIdAsync(userId);
        if (cart != null)
        {
            await _unitOfWork.ShoppingCarts.ClearAsync(cart.Id);
            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Cart cleared for UserId: {UserId}", userId);
        }
    }
}
