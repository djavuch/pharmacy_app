using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PharmacyApp.Application.DTOs.User.AccountDto;
using PharmacyApp.Application.DTOs.User.Enums;
using PharmacyApp.Application.Interfaces.Services;
using PharmacyApp.Application.Services;
using PharmacyApp.Presentation.Helpers;
using System.Security.Claims;
using static PharmacyApp.Domain.Exceptions.AppExceptions;
namespace PharmacyApp.Presentation.Controllers;

[ApiController]
[Route("account/")]
public class AccountController : ControllerBase
{
    private readonly IAuthService _userService;
    private readonly IShoppingCartService _shoppingCartService;
    private readonly ILogger<AccountController> _logger;


    public AccountController(IAuthService userService,
        IShoppingCartService shoppingCartService,
        ILogger<AccountController> logger)
    {
        _userService = userService;
        _shoppingCartService = shoppingCartService;
        _logger = logger;

    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(UserRegistrationDto userRegistrationDto)
    {
        var result = await _userService.UserRegisterAsync(userRegistrationDto, Request.Scheme, Request.Host.Value);

        var sessionId = SessionHelper.GetSessionId(HttpContext);
        if (!string.IsNullOrEmpty(sessionId) && !string.IsNullOrEmpty(result.Id))
        {
            await _shoppingCartService.MergeCartsOnLoginAsync(sessionId, result.Id);
            SessionHelper.ClearSessionId(HttpContext);         
        }

        return CreatedAtAction(nameof(Register), new { userId = result.Id }, result); // Return 201 Created with user details
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
                _ => StatusCode(401, new { message = result.Message })
            };
        }

        var sessionId = SessionHelper.GetSessionId(HttpContext);

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
        if (string.IsNullOrEmpty(request?.RefreshToken))
        {
            throw new BadRequestException("Refresh token is required for logout.");
        }

        await _userService.LogoutAsync(request.RefreshToken);
        return Ok(new { message = "Logged out successfully." });
    }

    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmailAsync(string userId, string token)
    {
        var result = await _userService.ConfirmEmailAsync(userId, token);

        if (!result)
            throw new BadRequestException($"Email confirmation failed.");

        return Ok(new { message = "Email confirmed successfully." });
    }

    [HttpPost("resend-confirmation")]
    public async Task<IActionResult> ResendConfirmationEmail(string email)
    {
        await _userService.ResendConfirmationEmailAsync(email, Request.Scheme, Request.Host.Value);

        return Ok(new { message = "If an account with that email exists, a confirmation link has been sent." });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordDto forgotPasswordDto)
    {
        await _userService.ForgotPasswordAsync(forgotPasswordDto.Email, Request.Scheme, Request.Host.Value);
        return Ok(new { message = "If an account with that email exists, a password reset link has been sent." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordDto resetPasswordDto)
    {
        var result = await _userService.ResetPasswordAsync(resetPasswordDto);

        if (!result.Succeeded)
            throw new BadRequestException("Password reset failed.");

        return Ok(new { message = "Password has been reset successfully." });
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        var result = await _userService.RefreshTokenAsync(request.RefreshToken);

        if (!result.Succeeded || result.Token is null || result.RefreshToken is null)
        {
            throw new UnauthorizedException("Token refresh failed.");
        }

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
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        changePasswordDto.UserId = userId;
        
        await _userService.ChangePasswordAsync(changePasswordDto);

        return Ok(new { message = "Password changed successfully." });
    }
}
