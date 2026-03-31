using Microsoft.AspNetCore.Mvc;
using PharmacyApp.Application.Interfaces.Services;
using System.Security.Claims;
using PharmacyApp.Application.DTOs.User.UserProfileDto;
using static PharmacyApp.Domain.Exceptions.AppExceptions;

namespace PharmacyApp.Presentation.Controllers;

[ApiController]
[Route("users/")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("my-profile")]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var userProfile = await _userService.GetCurrentUserProfileAsync(userId);

        if (userProfile == null)
        {
            return NotFound();
        }

        return Ok(userProfile);
    }
    
    [HttpPut("my-profile")]
    public async Task<IActionResult> UpdateMyProfile(UpdateUserDto updateUserDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    
        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedException("You must be logged in to update your profile");
        }
        
        if (updateUserDto.UserId != userId)
        {
            throw new ForbiddenException("You can only update your own profile");
        }
    
        await _userService.UpdateUserProfileAsync(updateUserDto);
    
        return NoContent();
    }
    

    [HttpGet("my-profile/orders")]
    public async Task<IActionResult> GetMyOrders(int pageIndex = 1, int pageSize = 10)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var orders = await _userService.GetUserOrdersAsync(userId, pageIndex, pageSize);

        return Ok(orders);
    }

    [HttpGet("my-profile/reviews")]
    public async Task<IActionResult> GetMyReviews(int pageIndex = 1, int pageSize = 10)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var reviews = await _userService.GetUserReviewsAsync(userId, pageIndex, pageSize);
        return Ok(reviews);
    }
}