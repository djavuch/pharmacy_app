using PharmacyApp.Domain.Entities;

namespace PharmacyApp.Application.Interfaces.Repositories;

public interface IWishlistRepository
{
    Task<List<Wishlist>> GetByUserIdAsync(string userId);
    Task AddAsync(Wishlist wishlist);
    Task<bool> RemoveAsync(string userId, int productId);
    Task<bool> IsProductInWishlistAsync(string userId, int productId);
    
    // Admin specific
    Task<List<Wishlist>> GetUsersByProductIdAsync(int productId);
}
