using PharmacyApp.Application.DTOs.Common;
using PharmacyApp.Domain.Entities;

namespace PharmacyApp.Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<UserModel?> GetByIdAsync(string userId);
    Task<UserModel?> GetByEmailAsync(string email); // needs to checking existing email during registration
    Task<UserModel?> GetCurrentProfileAsync(string userId);
    Task<PaginatedList<ReviewModel?>> GetCurrentReviews(string userId, int pageIndex, int pageSize);
    Task<PaginatedList<OrderModel?>> GetCurrentOrders(string userId, int pageIndex, int pageSize);
    Task UpdateAsync(UserModel user);
    Task<bool> AnyUserExistsAsync();

    // Admin specific
    IQueryable<UserModel> GetAllAsync();
}
