using Microsoft.EntityFrameworkCore;
using PharmacyApp.Application.Interfaces.Repositories;
using PharmacyApp.Domain.Entities;
using PharmacyApp.Infrastructure.Data;

namespace PharmacyApp.Infrastructure.Repositories;

public class WishlistRepository : IWishlistRepository
{
    private readonly PharmacyAppDbContext _dbContext;
    public WishlistRepository(PharmacyAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Wishlist>> GetByUserIdAsync(string userId)
    {
        return await _dbContext.Wishlists
            .Include(w => w.Product)
            .Where(w => w.UserId == userId)
            .ToListAsync();
    }

    public async Task<Wishlist> AddAsync(Wishlist wishlist)
    {
        await _dbContext.Wishlists.AddAsync(wishlist);
        await _dbContext.Entry(wishlist).Reference(w => w.Product).LoadAsync();
        return wishlist;
    }

    public async Task RemoveAsync(string userId, int productId)
    {
        var wishlistItem = await _dbContext.Wishlists
            .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);

        if (wishlistItem != null)
        {
            _dbContext.Wishlists.Remove(wishlistItem);
        }
    }

    public async Task<bool> IsProductInWishlistAsync(string userId, int productId)
    {
        return await _dbContext.Wishlists
            .AnyAsync(w => w.UserId == userId && w.ProductId == productId);
    }
    
    // Admin specific
    public async Task<List<Wishlist>> GetUsersByProductIdAsync(int productId)
    {
        return await _dbContext.Wishlists
            .Include(w => w.User)
            .Where(w => w.ProductId == productId)
            .OrderByDescending(w => w.DateAdded)
            .ToListAsync();
    }
}
