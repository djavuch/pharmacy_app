using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using PharmacyApp.Application.Interfaces.Services;
using PharmacyApp.Presentation.Helpers;
using System.Security.Claims;
using PharmacyApp.Application.Contracts.User.Account;
using PharmacyApp.Application.Contracts.User.Results;

namespace PharmacyApp.Presentation.Controllers;

[ApiController]
[EnableCors("AllowFrontend")]
[Route("account/")]
public class AccountController : ControllerBase
{
    private readonly IAuthService _userService;
    private readonly IShoppingCartService _shoppingCartService;
    private readonly IConfiguration _configuration;
    
    public AccountController(IAuthService userService,
        IShoppingCartService shoppingCartService,
        IConfiguration configuration)
    {
        _userService = userService;
        _shoppingCartService = shoppingCartService;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(UserRegistrationDto userRegistrationDto)
    {
        if (!TryResolvePublicUrlComponents(out var scheme, out var host))
            return BadRequest(new { message = "Request host is required" });
        
        var result = await _userService.UserRegisterAsync(userRegistrationDto, scheme, host);
        
        if (!result.IsSuccess)
            return StatusCode(result.ErrorType.ToStatusCode(), new { message = result.Message });

        var sessionId = SessionHelper.TryGetSessionId(HttpContext);
        var sessionOwnerUserId = SessionHelper.TryGetSessionOwnerUserId(HttpContext);
        var registeredUserId = result.Value!.Id;
        
        if (!string.IsNullOrEmpty(sessionId) &&
            !string.IsNullOrEmpty(registeredUserId) &&
            CanMergeSessionCartWithUser(sessionOwnerUserId, registeredUserId, out var replaceExistingItems))
        {
            await _shoppingCartService.MergeCartsOnLoginAsync(
                sessionId,
                registeredUserId,
                replaceExistingItems);
        }

        if (!string.IsNullOrEmpty(sessionId))
            SessionHelper.ClearSessionId(HttpContext);

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
                LoginFailureReason.AccountLocked => StatusCode(403, new
                {
                    message = result.Message,
                    accountLocked = true
                }),
                _ => StatusCode(401, new { message = result.Message })
            };
        }

        var sessionId = SessionHelper.TryGetSessionId(HttpContext);
        var sessionOwnerUserId = SessionHelper.TryGetSessionOwnerUserId(HttpContext);

        if (!string.IsNullOrEmpty(sessionId) &&
            !string.IsNullOrEmpty(result.UserId) &&
            CanMergeSessionCartWithUser(sessionOwnerUserId, result.UserId, out var replaceExistingItems))
        {
            await _shoppingCartService.MergeCartsOnLoginAsync(
                sessionId,
                result.UserId,
                replaceExistingItems);
        }

        if (!string.IsNullOrEmpty(sessionId))
            SessionHelper.ClearSessionId(HttpContext);

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

        var sessionId = SessionHelper.TryGetSessionId(HttpContext);

        if (!string.IsNullOrWhiteSpace(sessionId))
            await _shoppingCartService.ClearCartAsync(null, sessionId);

        SessionHelper.ClearSessionId(HttpContext);
        
        await _userService.LogoutAsync(request.RefreshToken);
        return Ok(new { message = "Logged out successfully." });
    }

    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmailAsync(string userId, string token)
    {
        var result = await _userService.ConfirmEmailAsync(userId, token);

        if (!result.IsSuccess)
            return StatusCode(result.ErrorType.ToStatusCode(), new { message = result.Message });

        return Ok(new { message = "Email confirmed successfully." });
    }

    [HttpPost("resend-confirmation")]
    public async Task<IActionResult> ResendConfirmationEmail([FromQuery] string email)
    {
        if (!TryResolvePublicUrlComponents(out var scheme, out var host))
            return BadRequest(new { message = "Request host is required" });
        
        var result = await _userService.ResendConfirmationEmailAsync(email, scheme, host);

        if (!result.IsSuccess)
            return StatusCode(result.ErrorType.ToStatusCode(), new { message = result.Message });
        
        return Ok(new { message = "If an account with that email exists, a confirmation link has been sent." });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordDto forgotPasswordDto)
    {
        if (!TryResolvePublicUrlComponents(out var scheme, out var host))
            return BadRequest(new { message = "Request host is required" });
        
        var result = await _userService.ForgotPasswordAsync(forgotPasswordDto.Email, scheme, host);
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

    private bool TryResolvePublicUrlComponents(out string scheme, out string host)
    {
        var frontendBaseUrl = _configuration["Frontend:BaseUrl"];
        if (!string.IsNullOrWhiteSpace(frontendBaseUrl) &&
            Uri.TryCreate(frontendBaseUrl, UriKind.Absolute, out var frontendUri))
        {
            scheme = frontendUri.Scheme;
            host = frontendUri.IsDefaultPort ? frontendUri.Host : frontendUri.Authority;
            return true;
        }

        if (Request.Host.HasValue)
        {
            scheme = Request.Scheme;
            host = Request.Host.Value;
            return true;
        }

        scheme = string.Empty;
        host = string.Empty;
        return false;
    }

    private static bool CanMergeSessionCartWithUser(
        string? sessionOwnerUserId,
        string userId,
        out bool replaceExistingItems)
    {
        replaceExistingItems = false;

        if (string.IsNullOrWhiteSpace(userId))
            return false;

        if (string.IsNullOrWhiteSpace(sessionOwnerUserId))
            return true;

        if (string.Equals(sessionOwnerUserId, userId, StringComparison.OrdinalIgnoreCase))
        {
            replaceExistingItems = true;
            return true;
        }

        return false;
    }
}
