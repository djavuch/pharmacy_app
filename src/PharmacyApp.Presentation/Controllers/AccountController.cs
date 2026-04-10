using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyApp.Application.Interfaces.Services;
using PharmacyApp.Presentation.Helpers;
using System.Security.Claims;
using PharmacyApp.Application.Contracts.User.Account;
using PharmacyApp.Application.Contracts.User.Results;

namespace PharmacyApp.Presentation.Controllers;

[ApiController]
[Route("account/")]
public class AccountController : ControllerBase
{
    private readonly IAuthService _userService;
    private readonly IShoppingCartService _shoppingCartService;
    
    public AccountController(IAuthService userService,
        IShoppingCartService shoppingCartService)
    {
        _userService = userService;
        _shoppingCartService = shoppingCartService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(UserRegistrationDto userRegistrationDto)
    {
        if (!Request.Host.HasValue)
            return BadRequest(new { message = "Request host is required" });
        
        var result = await _userService.UserRegisterAsync(userRegistrationDto, Request.Scheme, Request.Host.Value);
        
        if (!result.IsSuccess)
            return StatusCode(result.ErrorCode, new { message = result.Message });

        var sessionId = SessionHelper.TryGetSessionId(HttpContext);
        
        if (!string.IsNullOrEmpty(sessionId) && !string.IsNullOrEmpty(result.Value!.Id))
        {
            await _shoppingCartService.MergeCartsOnLoginAsync(sessionId, result.Value.Id);
            SessionHelper.ClearSessionId(HttpContext);         
        }

        return Ok(result.Value); 
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginDto userLoginDto)
    {
        var result = await _userService.LoginAsync(userLoginDto);

        if (!result.Succeeded)
        {
            return result.FailureReason switch
            {
                LoginFailureReason.EmailNotConfirmed => StatusCode(403, new
                {
                    message = result.Message,
                    requiresEmailConfirmation = true
                }),
                LoginFailureReason.PasswordResetRequired => StatusCode(403, new
                {
                    message = result.Message,
                    requiresPasswordReset = true
                }),
                LoginFailureReason.AccountLocked => StatusCode(403, new
                {
                    message = result.Message,
                    accountLocked = true
                }),
                _ => StatusCode(401, new { message = result.Message })
            };
        }

        var sessionId = SessionHelper.TryGetSessionId(HttpContext);

        if (!string.IsNullOrEmpty(sessionId) && !string.IsNullOrEmpty(result.UserId))
        {
            await _shoppingCartService.MergeCartsOnLoginAsync(sessionId, result.UserId);
            SessionHelper.ClearSessionId(HttpContext);
        }

        return Ok(new
        {
            token = result.Token,
            refreshToken = result.RefreshToken,
            userId = result.UserId
        });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(RefreshTokenRequestDto request)
    {
        if (string.IsNullOrEmpty(request.RefreshToken))
            return BadRequest(new { message = "Refresh token is required for logout." });
        
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (!string.IsNullOrEmpty(userId))
        {
            var sessionId = SessionHelper.GetOrCreateSessionId(HttpContext);
            await _shoppingCartService.MergeCartsOnLogoutAsync(userId, sessionId);
            
            await _shoppingCartService.ClearCartByUserIdAsync(userId);
        }
        
        await _userService.LogoutAsync(request.RefreshToken);
        return Ok(new { message = "Logged out successfully." });
    }

    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmailAsync(string userId, string token)
    {
        var result = await _userService.ConfirmEmailAsync(userId, token);

        if (!result.IsSuccess)
            return StatusCode(result.ErrorCode, new { message = result.Message });

        return Ok(new { message = "Email confirmed successfully." });
    }

    [HttpPost("resend-confirmation")]
    public async Task<IActionResult> ResendConfirmationEmail([FromQuery] string email)
    {
        if (!Request.Host.HasValue)
            return BadRequest(new { message = "Request host is required" });
        
        var result = await _userService.ResendConfirmationEmailAsync(email, Request.Scheme, Request.Host.Value);

        if (!result.IsSuccess)
            return StatusCode(result.ErrorCode, new { message = result.Message });
        
        return Ok(new { message = "If an account with that email exists, a confirmation link has been sent." });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordDto forgotPasswordDto)
    {
        if (!Request.Host.HasValue)
            return BadRequest(new { message = "Request host is required" });
        
        var result = await _userService.ForgotPasswordAsync(forgotPasswordDto.Email, Request.Scheme, Request.Host.Value);
        return Ok(new { message = result.Message });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordDto resetPasswordDto)
    {
        var result = await _userService.ResetPasswordAsync(resetPasswordDto);

        if (!result.Succeeded)
            return BadRequest(new { message = "Password reset failed." });

        return Ok(new { message = "Password has been reset successfully." });
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        var result = await _userService.RefreshTokenAsync(request.RefreshToken);

        if (!result.Succeeded || result.Token is null)
            return Unauthorized(new { message = "Token refresh failed." });

        return Ok(new
        {
            accessToken = result.Token,
            refreshToken = result.RefreshToken,
            message = result.Message
        });
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto changePasswordDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "User is not authenticated." });

        changePasswordDto.UserId = userId;

        var result = await _userService.ChangePasswordAsync(changePasswordDto);
        
        if (!result.Succeeded)
            return BadRequest(new { message = "Failed to change password." });
        
        return Ok(new { message = "Password changed successfully." });
    }
}