using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using PharmacyApp.Application.Interfaces.Repositories;
using PharmacyApp.Domain.Entities;

namespace PharmacyApp.Infrastructure.Repositories;
public class AuthRepository : IAuthRepository
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly ILogger<AuthRepository> _logger;

    public AuthRepository(UserManager<User> userManager, SignInManager<User> signInManager, ILogger<AuthRepository> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    public async Task<IdentityResult> RegisterAsync(User user, string password)
    {
        return await _userManager.CreateAsync(user, password);
    }

    public async Task<bool> CheckPasswordAsync(User user, string password)
    {
        return await _userManager.CheckPasswordAsync(user, password);
    }

    public async Task<SignInResult> CheckPasswordForSignInAsync(User user, string password, bool lockoutOnFailure)
    {
        return await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure);
    }

    public async Task LogoutAsync()
    {
        await _signInManager.SignOutAsync();
    }

    public async Task<string> GenerateEmailConfirmationTokenAsync(User user)
    {
        return await _userManager.GenerateEmailConfirmationTokenAsync(user);
    }

    public async Task<string> GeneratePasswordResetTokenAsync(User user)
    {
        return await _userManager.GeneratePasswordResetTokenAsync(user);
    }

    public async Task<IdentityResult> ConfirmEmailAsync(User user, string token)
    {
        return await _userManager.ConfirmEmailAsync(user, token);
    }

    public async Task<IdentityResult> ResetPasswordAsync(User user, string token, string newPassword)
    {
        return await _userManager.ResetPasswordAsync(user, token, newPassword);
    }

    public async  Task<IdentityResult> ChangePasswordAsync(User user, string currentPassword, string newPassword)
    {
        return await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
    }

    public async Task<IdentityResult> AddToRoleAsync(User user, string role)
    {
        return await _userManager.AddToRoleAsync(user, role);
    }

    public async Task<IdentityResult> UpdateSecurityStampAsync(User user)
    {
        return await _userManager.UpdateSecurityStampAsync(user);
    }
    
    // Admin specific
    public async Task LockUserAsync(User user, DateTimeOffset lockoutEnd)
    {
        await _userManager.SetLockoutEndDateAsync(user, lockoutEnd);
    }
    
    public async Task UnlockUserAsync(User user)
    {
        await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow);
        await _userManager.ResetAccessFailedCountAsync(user);
    }
}
