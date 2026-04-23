using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using PharmacyApp.Application.Interfaces.Services;
using System.Security.Claims;
using PharmacyApp.Application.Common.Pagination;
using PharmacyApp.Application.Contracts.User.Profile;
using static PharmacyApp.Domain.Exceptions.AppExceptions;

namespace PharmacyApp.Presentation.Controllers;

[ApiController]
[EnableCors("AllowFrontend")]
[Route("users/")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await _userService.GetCurrentUserProfileAsync(userId);
        
        if (!result.IsSuccess)
            return StatusCode(result.ErrorType.ToStatusCode(), new { message = result.Message });
        
        return Ok(result.Value);
    }
    
    [HttpPut("me")]
    public async Task<IActionResult> UpdateMyProfile(UpdateUserDto updateUserDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    
        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedException("You must be logged in to update your profile");
        }

        updateUserDto.UserId = userId;
        
        var result = await _userService.UpdateUserProfileAsync(updateUserDto);
        
        if (!result.IsSuccess)
            return StatusCode(result.ErrorType.ToStatusCode(), new { message = result.Message });
        
        return NoContent();
    }
    

    [HttpGet("me/orders")]
    public async Task<IActionResult> GetMyOrders([FromQuery] QueryParams query)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var orders = await _userService.GetUserOrdersAsync(userId, query);

        return Ok(orders);
    }

    [HttpGet("me/reviews")]
    public async Task<IActionResult> GetMyReviews([FromQuery] ReviewQueryParams queryParams)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var reviews = await _userService.GetUserReviewsAsync(userId, queryParams);
        return Ok(reviews);
    }
}
