using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using PharmacyApp.Application.Interfaces.Services;
using System.Security.Claims;
using PharmacyApp.Application.Common.Pagination;
using PharmacyApp.Application.Common.Results;
using PharmacyApp.Application.Contracts.User.Admin;

namespace PharmacyApp.Presentation.Controllers.Admin;

[ApiController]
[EnableCors("AllowFrontend")]
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
    public async Task<ApiResponse> GetAllUsers([FromQuery] QueryParams query)
    {
        var users = await _userService.GetAllUsersAsync(query);

        return new ApiResponse(true, null, users);
    }

    [HttpPost("{userId}/lock")]
    public async Task<ApiResponse> LockUser(string userId, [FromBody] LockUserDto? dto = null)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    
        if (currentUserId == userId)
            return new ApiResponse(false, "You cannot lock your own account", null);
    
        var result = await _userService.LockUserAsync(userId, dto?.LockoutEnd);
        
        if (!result.IsSuccess)
            return new ApiResponse(false, result.Message, null);
        
        return new ApiResponse(true, "User has been locked", result.Value);
    }

    [HttpPost("{userId}/unlock")]
    public async Task<ApiResponse> UnlockUser(string userId)
    {
        var result = await _userService.UnlockUserAsync(userId);
        
        if (!result.IsSuccess)
            return new ApiResponse(false, result.Message, null);
        
        return new ApiResponse(true, "User has been unlocked", result.Value);
    }
    
    [HttpPatch("{userId}/role")]
    public async Task<ApiResponse> ChangeUserRole(string userId, [FromBody] ChangeUserRoleDto dto)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (currentUserId == userId)
            return new ApiResponse(false, "You cannot change your own role", null);
        
        var result = await _userService.ChangeUserRoleAsync(userId, dto.Role);
        
        if (!result.IsSuccess)
            return new ApiResponse(false, result.Message, null);
        
        return new ApiResponse(true, "Role updated", result.Value);
    }
}
