using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PharmacyApp.Application.DTOs.Common;
using PharmacyApp.Application.Interfaces.Repositories;
using PharmacyApp.Domain.Entities;
using PharmacyApp.Infrastructure.Data;

namespace PharmacyApp.Infrastructure.Repositories;
public class UserRepository : IUserRepository
{
    private readonly PharmacyAppDbContext _dbContext;
    private readonly UserManager<UserModel> _userManager;   
    public UserRepository(PharmacyAppDbContext dbContext, UserManager<UserModel> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }
    public async Task<UserModel?> GetByIdAsync(string userId)
    {
        return await _userManager.FindByIdAsync(userId);
    }

    public async Task<UserModel?> GetByEmailAsync(string email)
    {
        return await _userManager.FindByEmailAsync(email);
    }

    public async Task<UserModel?> GetCurrentProfileAsync (string userId)
    {
        return await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId.ToString());
    }

    public async Task<PaginatedList<OrderModel?>> GetCurrentOrders(string userId, int pageIndex, int pageSize)
    {
        var query = _dbContext.Orders
            .AsNoTracking()
            .Include(o => o.OrderItems)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.OrderDate);
        
        return await PaginatedList<OrderModel?>.CreateAsync(query, pageIndex, pageSize);
    }

    public async Task<PaginatedList<ReviewModel?>> GetCurrentReviews(string userId, int pageIndex, int pageSize)
    {
        var query = _dbContext.Reviews
            .AsNoTracking()
            .Include(r => r.Product)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt);

        return await PaginatedList<ReviewModel?>.CreateAsync(query, pageIndex, pageSize);
    }

    public async Task UpdateAsync(UserModel user)
    {
        await _userManager.UpdateAsync(user);
    }

    public async Task<bool> AnyUserExistsAsync()
    {
        return await _dbContext.Users.AnyAsync();
    }

    // Admin specific
    public IQueryable<UserModel> GetAllAsync()
    {
        return _dbContext.Users
            .AsNoTracking()
            .AsQueryable();
    }

    public async Task<IdentityResult> RemoveFromRolesAsync(UserModel user, IEnumerable<string> roles)
    {
        return await _userManager.RemoveFromRolesAsync(user, roles);
    }

    public async Task<IList<string>> GetRolesAsync(UserModel user)
    {
        return await _userManager.GetRolesAsync(user);
    }
}