using PharmacyApp.Application.Common.Pagination;
using PharmacyApp.Application.Contracts.User.Admin;
using PharmacyApp.Application.Contracts.User.Profile;
using PharmacyApp.Domain.Common;

namespace PharmacyApp.Application.Interfaces.Services;

public interface IUserService 
{
    Task<Result<UserProfileDto>> GetCurrentUserProfileAsync(string userId);
    Task<PaginatedList<UserReviewSummaryDto>> GetUserReviewsAsync(string userId, ReviewQueryParams queryParams);
    Task<PaginatedList<UserOrderSummaryDto>> GetUserOrdersAsync(string userId, QueryParams query);
    Task<Result> UpdateUserProfileAsync(UpdateUserDto updateUserDto);

    // Admin specific
    Task<PaginatedList<AdminUserDto>> GetAllUsersAsync(QueryParams query);
    Task<Result<AdminUserDto>> LockUserAsync(string userId, DateTimeOffset? lockoutEnd = null);
    Task<Result<AdminUserDto>> UnlockUserAsync(string userId);
    Task<Result<AdminUserDto>> ChangeUserRoleAsync(string userId, string role);
}
