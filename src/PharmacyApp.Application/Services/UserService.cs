using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using PharmacyApp.Application.Common;
using PharmacyApp.Application.Common.Pagination;
using PharmacyApp.Application.Contracts.User.Profile;
using PharmacyApp.Application.Contracts.User.Admin;
using PharmacyApp.Application.Interfaces.Repositories;
using PharmacyApp.Application.Interfaces.Services;
using PharmacyApp.Application.Mappers;
using PharmacyApp.Domain.Common;

namespace PharmacyApp.Application.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWorkRepository _unitOfWork;
    private readonly HybridCache _cache;
    private static int _cacheVersion = 0;
    
    public UserService(IUnitOfWorkRepository unitOfWork, HybridCache cache)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<Result<UserProfileDto>> GetCurrentUserProfileAsync(string userId)
    {
        var userDto = await _cache.GetOrCreateAsync(
            CacheKeys.Users.Profile(_cacheVersion, userId),
            async _ =>
            {
                var user = await _unitOfWork.Users.GetCurrentProfileAsync(userId);

                if (user is null)
                    return null;
                
                return user.ToUserDto();
            },
            new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(15),
                LocalCacheExpiration = TimeSpan.FromMinutes(5)
            }
        );
        if (userDto is null)
            return Result<UserProfileDto>.NotFound("User not found");
        
        return Result<UserProfileDto>.Success(userDto);
    }

    public async Task<PaginatedList<UserOrderSummaryDto?>> GetUserOrdersAsync(string userId, QueryParams query)
    {
        return await _cache.GetOrCreateAsync(
            CacheKeys.Users.Orders(_cacheVersion, userId, query),
            async cancel =>
            {
                var orders = await _unitOfWork.Users.GetCurrentOrders(userId, query.PageIndex, query.PageSize);
                var userOrdersDtos = orders.Items.Select(o => o.ToUserOrdersDto()).ToList();
                return PaginatedList<UserOrderSummaryDto?>.Create(userOrdersDtos, orders.TotalPages, query);
            },
            new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(10),
                LocalCacheExpiration = TimeSpan.FromMinutes(2)
            }
        );
    }

    public async Task<PaginatedList<UserReviewSummaryDto?>> GetUserReviewsAsync(string userId, ReviewQueryParams queryParams)
    {
        return await _cache.GetOrCreateAsync(
            CacheKeys.Users.Reviews(_cacheVersion, userId, queryParams),
            async cancel =>
            {
                var reviews = await _unitOfWork.Users.GetCurrentReviews(userId, queryParams.PageIndex, queryParams.PageSize);
                var userReviewsDtos = reviews.Items.Select(r => r.ToUserReviewsDto()).ToList();
                
                return PaginatedList<UserReviewSummaryDto?>.Create(userReviewsDtos,  reviews.TotalPages, queryParams);
              
            },
            new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(10),
                LocalCacheExpiration = TimeSpan.FromMinutes(2)
            }
        );
    }

    public async Task<Result> UpdateUserProfileAsync(UpdateUserDto updateUserDto)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(updateUserDto.UserId);

        if (user is null)
           return Result.NotFound("User not found");
        
        user.FirstName = updateUserDto.FirstName;
        user.LastName = updateUserDto.LastName;
        user.Address = updateUserDto.Address;
        user.PhoneNumber = updateUserDto.PhoneNumber;
        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();
        
        InvalidateUserCache();
        return Result.Success();
    }

    // admin only

    public async Task<PaginatedList<AdminUserDto>> GetAllUsersAsync(QueryParams query)
    {
        return await _cache.GetOrCreateAsync(
            CacheKeys.Users.AllPaged(_cacheVersion, query),
            async cancel =>
            {
                var usersQuery = _unitOfWork.Users.GetAllAsync();

                // Filtering
                if (!string.IsNullOrWhiteSpace(query.FilterOn) && !string.IsNullOrWhiteSpace(query.FilterQuery))
                {
                    if (query.FilterOn.Equals("UserName", StringComparison.OrdinalIgnoreCase))
                    {
                        usersQuery = usersQuery.Where(u => u.UserName.ToLower().Contains(query.FilterQuery.ToLower()));
                    }

                    if (query.FilterOn.Equals("Email", StringComparison.OrdinalIgnoreCase))
                    {
                        usersQuery = usersQuery.Where(u => u.Email.ToLower().Contains(query.FilterQuery.ToLower()));
                    }

                    if (query.FilterOn.Equals("FirstName", StringComparison.OrdinalIgnoreCase))
                    {
                        usersQuery = usersQuery.Where(u => u.FirstName.ToLower().Contains(query.FilterQuery.ToLower()));
                    }

                    if (query.FilterOn.Equals("LastName", StringComparison.OrdinalIgnoreCase))
                    {
                        usersQuery = usersQuery.Where(u => u.LastName.ToLower().Contains(query.FilterQuery.ToLower()));
                    }

                    if (DateTime.TryParse(query.FilterQuery, out var dateOfBirth))
                    {
                        usersQuery = usersQuery.Where(u => u.DateOfBirth.Date == dateOfBirth.Date);
                    }

                    if (DateTime.TryParse(query.FilterQuery, out var createdAt))
                    {
                        usersQuery = usersQuery.Where(u => u.CreatedAt.Date == createdAt.Date);
                    }
                }

                var totalCount = await usersQuery.CountAsync();

                // Sorting
                if (!string.IsNullOrWhiteSpace(query.SortBy))
                {
                    if (query.SortBy.Equals("UserName", StringComparison.OrdinalIgnoreCase))
                    {
                        usersQuery = query.IsAscending ? usersQuery.OrderBy(u => u.UserName) : usersQuery.OrderByDescending(u => u.UserName);
                    }
                    else if (query.SortBy.Equals("Email", StringComparison.OrdinalIgnoreCase))
                    {
                        usersQuery = query.IsAscending ? usersQuery.OrderBy(u => u.Email) : usersQuery.OrderByDescending(u => u.Email);
                    }
                    else if (query.SortBy.Equals("FirstName", StringComparison.OrdinalIgnoreCase))
                    {
                        usersQuery = query.IsAscending ? usersQuery.OrderBy(u => u.FirstName) : usersQuery.OrderByDescending(u => u.FirstName);
                    }
                    else if (query.SortBy.Equals("LastName", StringComparison.OrdinalIgnoreCase))
                    {
                        usersQuery = query.IsAscending ? usersQuery.OrderBy(u => u.LastName) : usersQuery.OrderByDescending(u => u.LastName);
                    }
                    else if (query.SortBy.Equals("DateOfBirth", StringComparison.OrdinalIgnoreCase))
                    {
                        usersQuery = query.IsAscending ? usersQuery.OrderBy(u => u.DateOfBirth) : usersQuery.OrderByDescending(u => u.DateOfBirth);
                    }
                    else if (query.SortBy.Equals("CreatedAt", StringComparison.OrdinalIgnoreCase))
                    {
                        usersQuery = query.IsAscending ? usersQuery.OrderBy(u => u.CreatedAt) : usersQuery.OrderByDescending(u => u.CreatedAt);
                    }
                }
                
                var users = await usersQuery
                    .Skip((query.PageIndex - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .ToListAsync();
                
                var userDtos = new List<AdminUserDto>();
                
                foreach (var user in users)
                {
                    var roles = await _unitOfWork.Users.GetRolesAsync(user);
                    var userDto = user.ToAdminUserDto(roles.FirstOrDefault()); 
                    userDtos.Add(userDto);
                }
                
                return PaginatedList<AdminUserDto>.Create(userDtos, totalCount, query);
            },
            new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(10),
                LocalCacheExpiration = TimeSpan.FromMinutes(10)
            }
        );
    }
    
    public async Task<Result<AdminUserDto>> LockUserAsync(string userId, DateTimeOffset? lockoutEnd = null)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user is null)
            return Result<AdminUserDto>.NotFound("User not found");

        // Lock for 30 days or a specified period
        var lockoutEndDate = lockoutEnd ?? DateTimeOffset.UtcNow.AddDays(30);
    
        await _unitOfWork.Auth.LockUserAsync(user, lockoutEndDate);
    
        // Revoke all tokens of a blocked user
        await _unitOfWork.RefreshTokens.RevokeAllUserTokensAsync(userId);
    
        InvalidateUserCache();
    
        var roles = await _unitOfWork.Users.GetRolesAsync(user);
        return Result<AdminUserDto>.Success(user.ToAdminUserDto(roles.FirstOrDefault()));
    }

    public async Task<Result<AdminUserDto>> UnlockUserAsync(string userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user is null)
            return Result<AdminUserDto>.NotFound("User not found");
        
        await _unitOfWork.Auth.UnlockUserAsync(user);
    
        InvalidateUserCache();
    
        var roles = await _unitOfWork.Users.GetRolesAsync(user);
        return Result<AdminUserDto>.Success(user.ToAdminUserDto(roles.FirstOrDefault()));
    }

    public async Task<Result<AdminUserDto>> ChangeUserRoleAsync(string userId, string role)
    {
        if (string.IsNullOrWhiteSpace(role))
            return Result<AdminUserDto>.BadRequest("Role is required.");
        
        var allowedRoles = new[] {"Admin", "Pharmacist", "Manager", "Customer"};
        var normalizedRole = role.Trim();

        if (!allowedRoles.Contains(normalizedRole, StringComparer.OrdinalIgnoreCase))
            return Result<AdminUserDto>.BadRequest("Unsupported role");
        
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user is null)
            return Result<AdminUserDto>.NotFound("User not found");
        
        var currentRoles = await _unitOfWork.Users.GetRolesAsync(user);
        
        if (currentRoles.Contains("Admin", StringComparer.OrdinalIgnoreCase) 
            && !normalizedRole.Equals("Admin", StringComparison.OrdinalIgnoreCase))
        {
            // Check how many admins there
            var allUsers = _unitOfWork.Users.GetAllAsync();
            var adminCount = 0;
        
            await foreach (var u in allUsers.AsAsyncEnumerable())
            {
                var roles = await _unitOfWork.Users.GetRolesAsync(u);
                if (roles.Contains("Admin", StringComparer.OrdinalIgnoreCase))
                {
                    adminCount++;
                }
            }
        
            if (adminCount <= 1)
                return Result<AdminUserDto>.BadRequest("Cannot remove the last administrator from the system");
        }
        
        if (currentRoles.Count == 1 && currentRoles.Contains(normalizedRole, StringComparer.OrdinalIgnoreCase))
        {
            return Result<AdminUserDto>.Success(user.ToAdminUserDto(normalizedRole));
        }

        if (currentRoles.Count > 0)
        {
            var removeResult = await _unitOfWork.Users.RemoveFromRolesAsync(user, currentRoles);
            
            if (!removeResult.Succeeded)
                return Result<AdminUserDto>.BadRequest("Failed to remove current roles.");
        }
        
        var addResult = await _unitOfWork.Auth.AddToRoleAsync(user, normalizedRole);
        if (!addResult.Succeeded)
            return Result<AdminUserDto>.BadRequest("Failed to assign role.");
        
        await _unitOfWork.RefreshTokens.RevokeAllUserTokensAsync(userId);
        
        InvalidateUserCache();
        
        return Result<AdminUserDto>.Success(user.ToAdminUserDto(normalizedRole));
    }

    private static void InvalidateUserCache()
    {
        Interlocked.Increment(ref _cacheVersion);
    }
}
