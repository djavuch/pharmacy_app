using Microsoft.AspNetCore.Identity;
using PharmacyApp.Application.Common.Pagination;
using PharmacyApp.Domain.Entities;

namespace PharmacyApp.Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(string userId);
    Task<User?> GetByEmailAsync(string email); // needs to checking existing email during registration
    Task<User?> GetCurrentProfileAsync(string userId);
    Task<PaginatedList<Review>> GetCurrentReviews(string userId, int pageIndex, int pageSize);
    Task<PaginatedList<Order>> GetCurrentOrders(string userId, int pageIndex, int pageSize);
    Task UpdateAsync(User user);
    Task<bool> AnyUserExistsAsync();

    // Admin specific
    IQueryable<User> GetAllAsync();
    Task<IdentityResult> RemoveFromRolesAsync(User user, IEnumerable<string> roles);
    Task<IList<string>> GetRolesAsync(User user);
}
