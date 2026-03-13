using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyApp.Application.DTOs.Common;
using PharmacyApp.Application.Interfaces.Services;

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
}
