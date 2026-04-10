using Microsoft.AspNetCore.Identity;
using PharmacyApp.Application.Common.Results;
using PharmacyApp.Application.Contracts.User.Account;
using PharmacyApp.Application.Contracts.User.Profile;
using PharmacyApp.Application.Contracts.User.Results;
using PharmacyApp.Domain.Common;

namespace PharmacyApp.Application.Interfaces.Services;
public interface IAuthService
{
    Task<Result<UserProfileDto>> UserRegisterAsync(UserRegistrationDto userRegistrationDto, string scheme, string host);
    Task<Result<bool>> ConfirmEmailAsync(string userId, string token);
    Task<Result<bool>> ResendConfirmationEmailAsync(string email, string scheme, string host);
    Task<Result<bool>> ForgotPasswordAsync(string email, string scheme, string host);
    Task<IdentityOperationResult> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
    Task<LoginResult> LoginAsync(UserLoginDto userLoginDto);
    Task<LoginResult> RefreshTokenAsync(string refreshToken);
    Task LogoutAsync(string refreshToken);
    Task<IdentityOperationResult> ChangePasswordAsync(ChangePasswordDto changePasswordDto);
}
