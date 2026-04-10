using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PharmacyApp.Application.Common.Pagination;
using PharmacyApp.Application.Interfaces.Repositories;
using PharmacyApp.Domain.Entities;
using PharmacyApp.Infrastructure.Data;

namespace PharmacyApp.Infrastructure.Repositories;
public class UserRepository : IUserRepository
{
    private readonly PharmacyAppDbContext _dbContext;
    private readonly UserManager<User> _userManager;   
    public UserRepository(PharmacyAppDbContext dbContext, UserManager<User> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }
    public async Task<User?> GetByIdAsync(string userId)
    {
        return await _userManager.FindByIdAsync(userId);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _userManager.FindByEmailAsync(email);
    }

    public async Task<User?> GetCurrentProfileAsync (string userId)
    {
        return await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId.ToString());
    }

    public async Task<PaginatedList<Order?>> GetCurrentOrders(string userId, int pageIndex, int pageSize)
    {
        var query = _dbContext.Orders
            .AsNoTracking()
            .Include(o => o.OrderItems)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.OrderDate);
        
        return await PaginatedList<Order?>.CreateAsync(query, pageIndex, pageSize);
    }

    public async Task<PaginatedList<Review?>> GetCurrentReviews(string userId, int pageIndex, int pageSize)
    {
        var query = _dbContext.Reviews
            .AsNoTracking()
            .Include(r => r.Product)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt);

        return await PaginatedList<Review?>.CreateAsync(query, pageIndex, pageSize);
    }

    public async Task UpdateAsync(User user)
    {
        await _userManager.UpdateAsync(user);
    }

    public async Task<bool> AnyUserExistsAsync()
    {
        return await _dbContext.Users.AnyAsync();
    }

    // Admin specific
    public IQueryable<User> GetAllAsync()
    {
        return _dbContext.Users
            .AsNoTracking()
            .AsQueryable();
    }

    public async Task<IdentityResult> RemoveFromRolesAsync(User user, IEnumerable<string> roles)
    {
        return await _userManager.RemoveFromRolesAsync(user, roles);
    }

    public async Task<IList<string>> GetRolesAsync(User user)
    {
        return await _userManager.GetRolesAsync(user);
    }
}