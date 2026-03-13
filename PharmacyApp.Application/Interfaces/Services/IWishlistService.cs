using PharmacyApp.Application.DTOs.Wishlist;

namespace PharmacyApp.Application.Interfaces.Services;

public interface IWishlistService
{
    public Task<List<WishlistDto>> GetWishlistByUserIdAsync(string userId);
    public Task<WishlistDto> AddToWishlistAsync(WishlistDto wishlistDto, string userId);
    public Task RemoveFromWishlistAsync(string userId, int productId);
}
