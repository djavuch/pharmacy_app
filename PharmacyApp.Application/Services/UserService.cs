using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using PharmacyApp.Application.DTOs.Common;
using PharmacyApp.Application.DTOs.User.UserProfileDto;
using PharmacyApp.Application.Interfaces;
using PharmacyApp.Application.Interfaces.Services;
using PharmacyApp.Application.Mappers;
using static PharmacyApp.Domain.Exceptions.AppExceptions;

namespace PharmacyApp.Application.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWorkRepository _unitOfWork;
    private readonly HybridCache _cache;
    
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
            $"user:profile:{userId}",
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
            $"user:orders:{userId}:{pageIndex}:{pageSize}",
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
            $"user:reviews:{userId}:{pageIndex}:{pageSize}",
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
    }

    // admin only

    public async Task<PaginatedList<UserDto>> GetAllUsersAsync(int pageIndex = 1, int pageSize = 10,
        string? filterOn = null, string? filterQuery = null, string? sortBy = null, bool isAscending = true)
    {
        return await _cache.GetOrCreateAsync(
            $"users:all:{pageIndex}:{pageSize}:{filterOn}:{filterQuery}:{sortBy}:{isAscending}",
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

                return new PaginatedList<UserDto>
                {
                    PageIndex = pageIndex,
                    PageSize = pageSize,
                    TotalPages = totalCount,
                    Items = users.Select(u => u.ToUserDto()).ToList()
                };
            },
            new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(10),
                LocalCacheExpiration = TimeSpan.FromMinutes(10)
            }
        );
    }
}
