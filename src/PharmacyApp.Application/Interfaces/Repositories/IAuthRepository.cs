using Microsoft.AspNetCore.Identity;
using PharmacyApp.Domain.Entities;

namespace PharmacyApp.Application.Interfaces.Repositories;
public interface IAuthRepository
{
    Task<IdentityResult> RegisterAsync(UserModel user, string password);
    Task<bool> CheckPasswordAsync(UserModel user, string password);
    Task LogoutAsync();
    Task<string> GenerateEmailConfirmationTokenAsync(UserModel user);
    Task<string> GeneratePasswordResetTokenAsync(UserModel user);
    Task<IdentityResult> ConfirmEmailAsync(UserModel user, string token);
    Task<IdentityResult> ResetPasswordAsync(UserModel user, string token, string newPassword);
    Task<IdentityResult> ChangePasswordAsync(UserModel user, string currentPassword, string newPassword);
    Task<IdentityResult> AddToRoleAsync(UserModel user, string role);
    Task<IdentityResult> UpdateSecurityStampAsync(UserModel user);
    
    // Admin specific
    Task LockUserAsync(UserModel user, DateTimeOffset lockoutEnd);
    Task UnlockUserAsync(UserModel user);
}
