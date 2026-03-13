using Microsoft.AspNetCore.Identity;
using PharmacyApp.Domain.Entities;

namespace PharmacyApp.Application.Interfaces.Repositories;
public interface IAuthRepository
{
    Task<IdentityResult> RegisterAsync(UserModel user, string password);
    Task<SignInResult> LoginAsync(UserModel user, string password, bool rememberMe);
    Task<bool> CheckPasswordAsync(UserModel user, string password);
    Task UnlockUserAsync(UserModel user);
    Task LogoutAsync();
    Task<string> GenerateEmailConfirmationTokenAsync(UserModel user);
    Task<string> GeneratePasswordResetTokenAsync(UserModel user);
    Task<IdentityResult> ConfirmEmailAsync(UserModel user, string token);
    Task<IdentityResult> ResetPasswordAsync(UserModel user, string token, string newPassword);
    Task<IdentityResult> ChangePasswordAsync(UserModel user, string currentPassword, string newPassword);
    Task<IdentityResult> AddToRoleAsync(UserModel User, string role);
    Task<IList<string>> GetRolesAsync(UserModel user);
}
