using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using PharmacyApp.Application.DTOs.Common;
using PharmacyApp.Application.DTOs.User.UserProfileDto;
using PharmacyApp.Application.Interfaces;
using PharmacyApp.Application.Interfaces.Services;
using PharmacyApp.Application.Mappers;
using PharmacyApp.Domain.Entities;
using static PharmacyApp.Domain.Exceptions.AppExceptions;

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

    public async Task<UserDto?> GetUserByIdAsync(string userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user is null)
        {
            return null;
        }
        return user.ToUserDto();
    }

    public async Task<UserDto?> GetCurrentUserProfileAsync(string userId)
    {
        return await _cache.GetOrCreateAsync(
            $"user_v{_cacheVersion}:profile:{userId}",
            async cancel =>
            {
                var user = await _unitOfWork.Users.GetCurrentProfileAsync(userId);

                if (user is null)
                {
                    return null;
                }

                return user.ToUserDto();
            },
            new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(15),
                LocalCacheExpiration = TimeSpan.FromMinutes(5)
            }
        );
    }

    public async Task<PaginatedList<UserOrdersDto?>> GetUserOrdersAsync(string userId, int pageIndex, int pageSize)
    {
        return await _cache.GetOrCreateAsync(
            $"user_v{_cacheVersion}:orders:{userId}:{pageIndex}:{pageSize}",
            async cancel =>
            {
                var orders = await _unitOfWork.Users.GetCurrentOrders(userId, pageIndex, pageSize);
                var userOrdersDtos = orders.Items.Select(o => o.ToUserOrdersDto()).ToList();
                return new PaginatedList<UserOrdersDto?>
                {
                    Items = userOrdersDtos,
                    PageIndex = orders.PageIndex,
                    PageSize = pageSize,
                    TotalPages = orders.TotalPages
                };
            },
            new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(10),
                LocalCacheExpiration = TimeSpan.FromMinutes(2)
            }
        );
    }

    public async Task<PaginatedList<UserReviewsDto?>> GetUserReviewsAsync(string userId, int pageIndex, int pageSize)
    {
        return await _cache.GetOrCreateAsync(
            $"user_v{_cacheVersion}:reviews:{userId}:{pageIndex}:{pageSize}",
            async cancel =>
            {
                var reviews = await _unitOfWork.Users.GetCurrentReviews(userId, pageIndex, pageSize);
                var userReviewsDtos = reviews.Items.Select(r => r.ToUserReviewsDto()).ToList();
                return new PaginatedList<UserReviewsDto?>
                {
                    Items = userReviewsDtos,
                    PageIndex = reviews.PageIndex,
                    PageSize = pageSize,
                    TotalPages = reviews.TotalPages
                };
            },
            new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(10),
                LocalCacheExpiration = TimeSpan.FromMinutes(2)
            }
        );
    }

    public async Task UpdateUserProfileAsync(UpdateUserDto updateUserDto)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(updateUserDto.UserId);

        if (user is null)
        {
            throw new NotFoundException("User not found");
        }

        user.FirstName = updateUserDto.FirstName;
        user.LastName = updateUserDto.LastName;
        user.Address = updateUserDto.Address;
        user.PhoneNumber = updateUserDto.PhoneNumber;
        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();
        
        InvalidateUserCache();
    }

    // admin only

    public async Task<PaginatedList<UserDto>> GetAllUsersAsync(int pageIndex = 1, int pageSize = 10,
        string? filterOn = null, string? filterQuery = null, string? sortBy = null, bool isAscending = true)
    {
        return await _cache.GetOrCreateAsync(
            $"users_v{_cacheVersion}:all:{pageIndex}:{pageSize}:{filterOn}:{filterQuery}:{sortBy}:{isAscending}",
            async cancel =>
            {
                var usersQuery = _unitOfWork.Users.GetAllAsync();

                // Filtering
                if (!string.IsNullOrWhiteSpace(filterOn) && !string.IsNullOrWhiteSpace(filterQuery))
                {
                    if (filterOn.Equals("UserName", StringComparison.OrdinalIgnoreCase))
                    {
                        usersQuery = usersQuery.Where(u => u.UserName.ToLower().Contains(filterQuery.ToLower()));
                    }

                    if (filterOn.Equals("Email", StringComparison.OrdinalIgnoreCase))
                    {
                        usersQuery = usersQuery.Where(u => u.Email.ToLower().Contains(filterQuery.ToLower()));
                    }

                    if (filterOn.Equals("FirstName", StringComparison.OrdinalIgnoreCase))
                    {
                        usersQuery = usersQuery.Where(u => u.FirstName.ToLower().Contains(filterQuery.ToLower()));
                    }

                    if (filterOn.Equals("LastName", StringComparison.OrdinalIgnoreCase))
                    {
                        usersQuery = usersQuery.Where(u => u.LastName.ToLower().Contains(filterQuery.ToLower()));
                    }

                    if (DateTime.TryParse(filterQuery, out var dateOfBirth))
                    {
                        usersQuery = usersQuery.Where(u => u.DateOfBirth.Date == dateOfBirth.Date);
                    }

                    if (DateTime.TryParse(filterQuery, out var createdAt))
                    {
                        usersQuery = usersQuery.Where(u => u.CreatedAt.Date == createdAt.Date);
                    }
                }

                var totalCount = await usersQuery.CountAsync();

                // Sorting
                if (!string.IsNullOrWhiteSpace(sortBy))
                {
                    if (sortBy.Equals("UserName", StringComparison.OrdinalIgnoreCase))
                    {
                        usersQuery = isAscending ? usersQuery.OrderBy(u => u.UserName) : usersQuery.OrderByDescending(u => u.UserName);
                    }
                    else if (sortBy.Equals("Email", StringComparison.OrdinalIgnoreCase))
                    {
                        usersQuery = isAscending ? usersQuery.OrderBy(u => u.Email) : usersQuery.OrderByDescending(u => u.Email);
                    }
                    else if (sortBy.Equals("FirstName", StringComparison.OrdinalIgnoreCase))
                    {
                        usersQuery = isAscending ? usersQuery.OrderBy(u => u.FirstName) : usersQuery.OrderByDescending(u => u.FirstName);
                    }
                    else if (sortBy.Equals("LastName", StringComparison.OrdinalIgnoreCase))
                    {
                        usersQuery = isAscending ? usersQuery.OrderBy(u => u.LastName) : usersQuery.OrderByDescending(u => u.LastName);
                    }
                    else if (sortBy.Equals("DateOfBirth", StringComparison.OrdinalIgnoreCase))
                    {
                        usersQuery = isAscending ? usersQuery.OrderBy(u => u.DateOfBirth) : usersQuery.OrderByDescending(u => u.DateOfBirth);
                    }
                    else if (sortBy.Equals("CreatedAt", StringComparison.OrdinalIgnoreCase))
                    {
                        usersQuery = isAscending ? usersQuery.OrderBy(u => u.CreatedAt) : usersQuery.OrderByDescending(u => u.CreatedAt);
                    }
                }

                var skipResults = (pageIndex - 1) * pageSize;

                var users = await usersQuery.Skip(skipResults).Take(pageSize).ToListAsync();
                
                var userDtos = new List<UserDto>();
                foreach (var user in users)
                {
                    var roles = await _unitOfWork.Users.GetRolesAsync(user);
                    var userDto = user.ToUserDto(roles.FirstOrDefault()); 
                    userDtos.Add(userDto);
                }

                return new PaginatedList<UserDto>
                {
                    PageIndex = pageIndex,
                    PageSize = pageSize,
                    TotalPages = totalCount,
                    Items = userDtos
                };
            },
            new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(10),
                LocalCacheExpiration = TimeSpan.FromMinutes(10)
            }
        );
    }
    
    public async Task<UserDto> LockUserAsync(string userId, DateTimeOffset? lockoutEnd = null)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user is null)
        {
            throw new NotFoundException("User not found");
        }

        // Lock for 30 days or a specified period
        var lockoutEndDate = lockoutEnd ?? DateTimeOffset.UtcNow.AddDays(30);
    
        await _unitOfWork.Auth.LockUserAsync(user, lockoutEndDate);
    
        // Revoke all tokens of a blocked user
        await _unitOfWork.RefreshTokens.RevokeAllUserTokensAsync(userId);
    
        InvalidateUserCache();
    
        var roles = await _unitOfWork.Users.GetRolesAsync(user);
        return user.ToUserDto(roles.FirstOrDefault());
    }

    public async Task<UserDto> UnlockUserAsync(string userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user is null)
        {
            throw new NotFoundException("User not found");
        }

        await _unitOfWork.Auth.UnlockUserAsync(user);
    
        InvalidateUserCache();
    
        var roles = await _unitOfWork.Users.GetRolesAsync(user);
        return user.ToUserDto(roles.FirstOrDefault());
    }

    public async Task<UserDto> ChangeUserRoleAsync(string userId, string role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            throw new BadRequestException("Role is required.");
        }
        
        var allowedRoles = new[] {"Admin", "Pharmacist", "Manager", "Customer"};
        var normalizedRole = role.Trim();

        if (!allowedRoles.Contains(normalizedRole, StringComparer.OrdinalIgnoreCase))
        {
            throw new BadRequestException("Unsupported role");
        }
        
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user is null)
        {
            throw new NotFoundException("User not found");
        }
        
        var currentRoles = await _unitOfWork.Users.GetRolesAsync(user);
        
        // An additional check that will prevent the removal of the last admin from the app
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
            {
                throw new BadRequestException("Cannot remove the last administrator from the system");
            }
        }
        
        if (currentRoles.Count == 1 && currentRoles.Contains(normalizedRole, StringComparer.OrdinalIgnoreCase))
        {
            return user.ToUserDto(normalizedRole);
        }

        if (currentRoles.Count > 0)
        {
            var removeResult = await _unitOfWork.Users.RemoveFromRolesAsync(user, currentRoles);
            
            if (!removeResult.Succeeded)
            {
                throw new BadRequestException("Failed to remove current roles.");
            }
        }
        
        var addResult = await _unitOfWork.Auth.AddToRoleAsync(user, normalizedRole);
        if (!addResult.Succeeded)
        {
            throw new BadRequestException("Failed to assign role.");
        }
        
        await _unitOfWork.RefreshTokens.RevokeAllUserTokensAsync(userId);
        
        InvalidateUserCache();
        
        return user.ToUserDto(normalizedRole);
    }

    private static void InvalidateUserCache()
    {
        Interlocked.Increment(ref _cacheVersion);
    }
}
