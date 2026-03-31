using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyApp.Application.DTOs.Admin.User;
using PharmacyApp.Application.DTOs.Common;
using PharmacyApp.Application.Interfaces.Services;
using System.Security.Claims;
using static PharmacyApp.Domain.Exceptions.AppExceptions;

namespace PharmacyApp.Presentation.Controllers.Admin;

[ApiController]
[Area("Admin")]
[Route("admin/users")]
[Authorize(Roles = "Admin")]
public class AdminUserController : ControllerBase
{
    private readonly IUserService _userService;

    public AdminUserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<ApiResponse> GetAllUsers(
        string? filterOn = null,
        string? filterQuery = null,
        bool? isAscending = null,
        string? sortBy = null,
        int pageIndex = 1,
        int pageSize = 10)
    {
        var users = await _userService.GetAllUsersAsync(
            pageIndex,
            pageSize,
            filterOn,
            filterQuery,
            sortBy,
            isAscending ?? true);

        return new ApiResponse(true, null, users);
    }

    [HttpPost("{userId}/lock")]
    public async Task<ApiResponse> LockUser(string userId, [FromBody] LockUserDto? dto = null)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    
        if (currentUserId == userId)
        {
            throw new BadRequestException("You cannot lock your own account");
        }
    
        var lockedUser = await _userService.LockUserAsync(userId, dto?.LockoutEnd);
        return new ApiResponse(true, "User has been locked", lockedUser);
    }

    [HttpPost("{userId}/unlock")]
    public async Task<ApiResponse> UnlockUser(string userId)
    {
        var unlockedUser = await _userService.UnlockUserAsync(userId);
        return new ApiResponse(true, "User has been unlocked", unlockedUser);
    }
    
    [HttpPatch("{userId}/role")]
    public async Task<ApiResponse> ChangeUserRole(string userId, [FromBody] ChangeUserRoleDto dto)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (currentUserId == userId)
        {
            throw new BadRequestException("You cannot change your own role");
        }
        
        var updatedUser = await _userService.ChangeUserRoleAsync(userId, dto.Role); 
        return new ApiResponse(true, "Role updated", updatedUser); 
    }
}
