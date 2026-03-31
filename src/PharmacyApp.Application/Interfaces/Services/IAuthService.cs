using PharmacyApp.Application.DTOs.User.AccountDto;
using PharmacyApp.Application.DTOs.User.UserProfileDto;

namespace PharmacyApp.Application.Interfaces.Services;
public interface IAuthService
{
    Task<UserDto?> UserRegisterAsync(UserRegistrationDto userRegistrationDto, string scheme, string host);
    Task<bool> ConfirmEmailAsync(string userId, string token);
    Task<bool> ResendConfirmationEmailAsync(string email, string scheme, string host);
    Task<bool> ForgotPasswordAsync(string email, string scheme, string host);
    Task<IdentityResultDto> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
    Task<LoginResultDto> LoginAsync(UserLoginDto userLoginDto);
    Task<LoginResultDto> RefreshTokenAsync(string refreshToken);
    Task LogoutAsync(string refreshToken);
    Task<IdentityResultDto> ChangePasswordAsync(ChangePasswordDto changePasswordDto);
}
