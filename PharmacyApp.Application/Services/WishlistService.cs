using PharmacyApp.Application.DTOs.Wishlist;
using PharmacyApp.Application.Interfaces;
using PharmacyApp.Application.Interfaces.Services;
using PharmacyApp.Application.Mappers;
using PharmacyApp.Domain.Entities;
using static PharmacyApp.Domain.Exceptions.AppExceptions;

namespace PharmacyApp.Application.Services;

public class WishlistService : IWishlistService
{
    private readonly IUnitOfWorkRepository _unitOfWork;
    public WishlistService(IUnitOfWorkRepository unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<WishlistDto>> GetWishlistByUserIdAsync(string userId)
    {
        var wishlistItems = await _unitOfWork.Wishlists.GetByUserIdAsync(userId);

        return wishlistItems.Select(w => w.ToWishlistDto()).ToList();
    }

    public async Task<WishlistDto> AddToWishlistAsync(WishlistDto wishlistDto, string userId)
    {
        var existingItem = await _unitOfWork.Wishlists.IsProductInWishlistAsync(userId, wishlistDto.ProductId);

        if (existingItem)
        {
            throw new ConflictException("Product is already in the wishlist.");
        }

        var wishlistItem = new WishlistModel
        {
            UserId = userId,
            ProductId = wishlistDto.ProductId
        };

        var addedWishlistItem = await _unitOfWork.Wishlists.AddAsync(wishlistItem);
        await _unitOfWork.SaveChangesAsync();

        return addedWishlistItem.ToWishlistDto();
    }

    public async Task RemoveFromWishlistAsync(string userId, int productId)
    {
        var wishlistItem = await _unitOfWork.Wishlists
            .IsProductInWishlistAsync(userId, productId);

        if (!wishlistItem)
        {
            throw new NotFoundException("Wishlist item", $"{userId}:{productId}"); // ← Бросаем из сервиса!
        }

        await _unitOfWork.Wishlists.RemoveAsync(userId, productId);
        await _unitOfWork.SaveChangesAsync();
    }
}
