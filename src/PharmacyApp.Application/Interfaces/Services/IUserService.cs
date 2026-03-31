using PharmacyApp.Application.DTOs.Common;
using PharmacyApp.Application.DTOs.User.UserProfileDto;

namespace PharmacyApp.Application.Interfaces.Services;

public interface IUserService 
{
    Task<UserDto?> GetUserByIdAsync(string userId);
    Task<UserDto?> GetCurrentUserProfileAsync(string userId);
    Task<PaginatedList<UserReviewsDto?>> GetUserReviewsAsync(string userId, int pageIndex, int pageSize);
    Task<PaginatedList<UserOrdersDto?>> GetUserOrdersAsync(string userId, int pageIndex, int pageSize);
    Task UpdateUserProfileAsync(UpdateUserDto updateUserDto);

    // Admin specific
    Task<PaginatedList<UserDto>> GetAllUsersAsync(int pageIndex = 1, int pageSize = 10, string? filterOn = null,
        string? filterQuery = null, string? sortBy = null, bool isAscending = true);
    Task<UserDto> LockUserAsync(string userId, DateTimeOffset? lockoutEnd = null);
    Task<UserDto> UnlockUserAsync(string userId);
    Task<UserDto> ChangeUserRoleAsync(string userId, string role);
}
