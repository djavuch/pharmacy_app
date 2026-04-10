using PharmacyApp.Application.Contracts.Wishlist;
using PharmacyApp.Application.Contracts.Wishlist.Admin;
using PharmacyApp.Domain.Common;

namespace PharmacyApp.Application.Interfaces.Services;

public interface IWishlistService
{
    public Task<List<WishlistDto>> GetWishlistByUserIdAsync(string userId);
    public Task<Result<WishlistDto>> AddToWishlistAsync(WishlistDto wishlistDto, string userId);
    public Task<Result> RemoveFromWishlistAsync(string userId, int productId);
    
    // Admin specific
    public Task<List<WishlistUserDto>> GetUsersWhoAddedProductAsync(int productId);
}
