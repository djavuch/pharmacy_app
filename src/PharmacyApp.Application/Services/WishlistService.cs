using Microsoft.Extensions.Caching.Hybrid;
using PharmacyApp.Application.Common;
using PharmacyApp.Application.Contracts.Wishlist;
using PharmacyApp.Application.Contracts.Wishlist.Admin;
using PharmacyApp.Application.Interfaces;
using PharmacyApp.Application.Interfaces.Repositories;
using PharmacyApp.Application.Interfaces.Services;
using PharmacyApp.Application.Mappers;
using PharmacyApp.Domain.Common;
using PharmacyApp.Domain.Entities;
using static PharmacyApp.Domain.Exceptions.AppExceptions;

namespace PharmacyApp.Application.Services;

public class WishlistService : IWishlistService
{
    private readonly IUnitOfWorkRepository _unitOfWork;
    private readonly HybridCache _cache;
    
    public WishlistService(IUnitOfWorkRepository unitOfWork, HybridCache cache)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<List<WishlistDto>> GetWishlistByUserIdAsync(string userId)
    {
        return await _cache.GetOrCreateAsync(
            CacheKeys.Wishlists.ByUser(userId),
            async _ =>
            {
                var items = await _unitOfWork.Wishlists.GetByUserIdAsync(userId);
                return items.Select(w => w.ToWishlistDto()).ToList();
            },
            new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(10),
                LocalCacheExpiration = TimeSpan.FromMinutes(5)
            });
    }

    public async Task<Result<WishlistDto>> AddToWishlistAsync(WishlistDto wishlistDto, string userId)
    {
        var existingItem = await _unitOfWork.Wishlists.IsProductInWishlistAsync(userId, wishlistDto.ProductId);

        if (existingItem)
            return Result<WishlistDto>.Conflict("Product is already in the wishlist.");

        var wishlistItem = new Wishlist
        {
            UserId = userId,
            ProductId = wishlistDto.ProductId
        };

        await using var transaction = await _unitOfWork.BeginTransactionAsync();

        try
        {
            await _unitOfWork.Wishlists.AddAsync(wishlistItem);
            await _unitOfWork.SaveChangesAsync();

            await _unitOfWork.Products.UpdateWishlistCountAsync(wishlistDto.ProductId, 1);
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        await _cache.RemoveAsync(CacheKeys.Wishlists.ByUser(userId));
        await _cache.RemoveAsync(CacheKeys.Wishlists.UsersByProduct(wishlistDto.ProductId));

        return Result<WishlistDto>.Success(new WishlistDto
        {
            ProductId = wishlistDto.ProductId,
            ProductName = wishlistDto.ProductName
        });
    }

    public async Task<Result> RemoveFromWishlistAsync(string userId, int productId)
    {
        await using var transaction = await _unitOfWork.BeginTransactionAsync();

        try
        {
            var removed = await _unitOfWork.Wishlists.RemoveAsync(userId, productId);

            if (!removed)
                return Result.NotFound("Product is not in the wishlist.");

            await _unitOfWork.Products.UpdateWishlistCountAsync(productId, -1);
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        await _cache.RemoveAsync(CacheKeys.Wishlists.ByUser(userId));
        await _cache.RemoveAsync(CacheKeys.Wishlists.UsersByProduct(productId));
        
        return Result.Success();
    }
    
    // Admin specific
    public async Task<List<WishlistUserDto>> GetUsersWhoAddedProductAsync(int productId)
    {
        return await _cache.GetOrCreateAsync(
            CacheKeys.Wishlists.UsersByProduct(productId),
            async _ =>
            {
                var wishlistItems = await _unitOfWork.Wishlists.GetUsersByProductIdAsync(productId);

                return wishlistItems.Select(w => new WishlistUserDto
                {
                    UserId = w.UserId,
                    UserEmail = w.User.Email,
                    UserFullName = $"{w.User.FirstName} {w.User.LastName}",
                    DateAdded = w.DateAdded
                }).ToList();
            },
            new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(10),
                LocalCacheExpiration = TimeSpan.FromMinutes(5)
            });
    }
}
