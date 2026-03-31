using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using PharmacyApp.Application.Interfaces.Repositories;
using PharmacyApp.Domain.Entities;

namespace PharmacyApp.Infrastructure.Repositories;
public class AuthRepository : IAuthRepository
{
    private readonly UserManager<UserModel> _userManager;
    private readonly SignInManager<UserModel> _signInManager;
    private readonly ILogger<AuthRepository> _logger;

    public AuthRepository(UserManager<UserModel> userManager, SignInManager<UserModel> signInManager, ILogger<AuthRepository> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    public async Task<IdentityResult> RegisterAsync(UserModel user, string password)
    {
        return await _userManager.CreateAsync(user, password);
    }

    public async Task<bool> CheckPasswordAsync(UserModel user, string password)
    {
        return await _userManager.CheckPasswordAsync(user, password);
    }

    public async Task LogoutAsync()
    {
        await _signInManager.SignOutAsync();
    }

    public async Task<string> GenerateEmailConfirmationTokenAsync(UserModel user)
    {
        return await _userManager.GenerateEmailConfirmationTokenAsync(user);
    }

    public async Task<string> GeneratePasswordResetTokenAsync(UserModel user)
    {
        return await _userManager.GeneratePasswordResetTokenAsync(user);
    }

    public async Task<IdentityResult> ConfirmEmailAsync(UserModel user, string token)
    {
        return await _userManager.ConfirmEmailAsync(user, token);
    }

    public async Task<IdentityResult> ResetPasswordAsync(UserModel user, string token, string newPassword)
    {
        return await _userManager.ResetPasswordAsync(user, token, newPassword);
    }

    public async  Task<IdentityResult> ChangePasswordAsync(UserModel user, string currentPassword, string newPassword)
    {
        return await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
    }

    public async Task<IdentityResult> AddToRoleAsync(UserModel user, string role)
    {
        return await _userManager.AddToRoleAsync(user, role);
    }

    public async Task<IdentityResult> UpdateSecurityStampAsync(UserModel user)
    {
        return await _userManager.UpdateSecurityStampAsync(user);
    }
    
    // Admin specific
    public async Task LockUserAsync(UserModel user, DateTimeOffset lockoutEnd)
    {
        await _userManager.SetLockoutEndDateAsync(user, lockoutEnd);
    }
    
    public async Task UnlockUserAsync(UserModel user)
    {
        await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow);
        await _userManager.ResetAccessFailedCountAsync(user);
    }
}
