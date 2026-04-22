using Microsoft.AspNetCore.Identity;
using PharmacyApp.Domain.Entities;

namespace PharmacyApp.Application.Interfaces.Repositories;
public interface IAuthRepository
{
    Task<IdentityResult> RegisterAsync(User user, string password);
    Task<bool> CheckPasswordAsync(User user, string password);
    Task<SignInResult> CheckPasswordForSignInAsync(User user, string password, bool lockoutOnFailure);
    Task LogoutAsync();
    Task<string> GenerateEmailConfirmationTokenAsync(User user);
    Task<string> GeneratePasswordResetTokenAsync(User user);
    Task<IdentityResult> ConfirmEmailAsync(User user, string token);
    Task<IdentityResult> ResetPasswordAsync(User user, string token, string newPassword);
    Task<IdentityResult> ChangePasswordAsync(User user, string currentPassword, string newPassword);
    Task<IdentityResult> AddToRoleAsync(User user, string role);
    Task<IdentityResult> UpdateSecurityStampAsync(User user);
    
    // Admin specific
    Task LockUserAsync(User user, DateTimeOffset lockoutEnd);
    Task UnlockUserAsync(User user);
}
