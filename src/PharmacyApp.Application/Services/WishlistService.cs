using PharmacyApp.Application.DTOs.Admin.Wishlist;
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
        
        await _unitOfWork.Products.UpdateWishlistCountAsync(wishlistDto.ProductId, 1);
        
        await _unitOfWork.SaveChangesAsync();

        return addedWishlistItem.ToWishlistDto();
    }

    public async Task RemoveFromWishlistAsync(string userId, int productId)
    {
        var wishlistItem = await _unitOfWork.Wishlists
            .IsProductInWishlistAsync(userId, productId);

        if (!wishlistItem)
        {
            throw new NotFoundException("Wishlist item", $"{userId}:{productId}"); 
        }

        await _unitOfWork.Wishlists.RemoveAsync(userId, productId);
        
        await _unitOfWork.Products.UpdateWishlistCountAsync(productId, -1);
        
        await _unitOfWork.SaveChangesAsync();
    }
    
    // Admin specific
    public async Task<List<WishlistUserDto>> GetUsersWhoAddedProductAsync(int productId)
    {
        var wishlistItems = await _unitOfWork.Wishlists.GetUsersByProductIdAsync(productId);
    
        return wishlistItems.Select(w => new WishlistUserDto
        {
            UserId = w.UserId,
            UserEmail = w.User.Email,
            UserFullName = $"{w.User.FirstName} {w.User.LastName}",
            DateAdded = w.DateAdded
        }).ToList();
    }
}
