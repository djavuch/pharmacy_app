using PharmacyApp.Domain.Entities;

namespace PharmacyApp.Application.Interfaces.Repositories;

public interface IWishlistRepository
{
    Task<List<WishlistModel>> GetByUserIdAsync(string userId);
    Task<WishlistModel> AddAsync(WishlistModel wishlist);
    Task RemoveAsync(string userId, int productId);
    Task<bool> IsProductInWishlistAsync(string userId, int productId);
    
    // Admin specific
    Task<List<WishlistModel>> GetUsersByProductIdAsync(int productId);
}