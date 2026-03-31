using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using PharmacyApp.Application.DTOs.User.AccountDto;
using PharmacyApp.Application.DTOs.User.Enums;
using PharmacyApp.Application.DTOs.User.UserProfileDto;
using PharmacyApp.Application.Interfaces;
using PharmacyApp.Application.Interfaces.Services; 
using PharmacyApp.Application.Mappers;
using PharmacyApp.Domain.Entities;
using System.Text;
using PharmacyApp.Application.Interfaces.Email;
using PharmacyApp.Infrastructure.Abstractions.Authentication;
using static PharmacyApp.Domain.Exceptions.AppExceptions;

namespace PharmacyApp.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWorkRepository _unitOfWork;
    private readonly IJwtTokenProvider _jwtTokenProvider;
    private readonly IClaimsService _claimsService;
    private readonly IBackgroundTaskQueue _taskQueue;
    private readonly IServiceProvider _serviceProvider;


    public AuthService(IUnitOfWorkRepository unitOfWork, IJwtTokenProvider jwtTokenProvider, 
        IClaimsService claimsService, IBackgroundTaskQueue taskQueue, 
        IServiceProvider serviceProvider)
    {
        _unitOfWork = unitOfWork;
        _jwtTokenProvider = jwtTokenProvider;
        _claimsService = claimsService;
        _taskQueue = taskQueue;
        _serviceProvider = serviceProvider;
    }

    public async Task<UserDto?> UserRegisterAsync(UserRegistrationDto userRegistrationDto, string scheme, string host)
    {
        var existingUser = await _unitOfWork.Users.GetByEmailAsync(userRegistrationDto.Email);

        if (existingUser is not null)
        {
            throw new ConflictException("User with this email already exists.");
        }
        
        var newUser = new UserModel
        {
            UserName = userRegistrationDto.Email,
            Email = userRegistrationDto.Email,
            FirstName = userRegistrationDto.FirstName,
            LastName = userRegistrationDto.LastName,
            DateOfBirth = DateTime.SpecifyKind(userRegistrationDto.DateOfBirth, DateTimeKind.Utc),
            PhoneNumber = userRegistrationDto.PhoneNumber,
            CreatedAt = DateTime.UtcNow
        }; 

        var result = await _unitOfWork.Auth.RegisterAsync(newUser, userRegistrationDto.Password);

        if (!result.Succeeded)
        {
            throw new BadRequestException("User registration failed: " + string.Join(", ", (IEnumerable<string>)result.Errors.Select(e => e.Description)));
        }
        
        await _unitOfWork.Auth.AddToRoleAsync(newUser, "Customer");
        

        var token = await _unitOfWork.Auth.GenerateEmailConfirmationTokenAsync(newUser);

        await _taskQueue.QueueBackgroundWorkItemAsync(async ct =>
        {
            using var scope = _serviceProvider.CreateScope();
            var emailService = scope.ServiceProvider.GetRequiredService<IAccountNotificationSender>();
            await emailService.SendEmailForRegisterConfirmationAsync(newUser, token, scheme, host, ct);
        });

        return newUser.ToUserDto(); 
    }

    public async Task<bool> ConfirmEmailAsync(string userId, string token)
    {
        if (userId is null || string.IsNullOrEmpty(token))
        {
            throw new BadRequestException("User ID and token are required for email confirmation.");
        }

        var user = await _unitOfWork.Users.GetByIdAsync(userId);

        if (user is null)
        {
            throw new BadRequestException("User not found or revoked.");
        }

        var decodedToken = WebEncoders.Base64UrlDecode(token);
        var normalizedToken = Encoding.UTF8.GetString(decodedToken);


        var result = await _unitOfWork.Auth.ConfirmEmailAsync(user, normalizedToken);

        return result.Succeeded;
    }

    public async Task<bool> ResendConfirmationEmailAsync(string email, string scheme, string host)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(email);

        if (user is null || user.EmailConfirmed)
        {
            return true;
        }

        var stampUpdateResult = await _unitOfWork.Auth.UpdateSecurityStampAsync(user);
        if (!stampUpdateResult.Succeeded)
        {
            throw new BadRequestException(
                "Failed to regenerate confirmation token: " + 
                string.Join(", ", stampUpdateResult.Errors.Select(e => e.Description)));
        }
        
        var token = await _unitOfWork.Auth.GenerateEmailConfirmationTokenAsync(user);

        await _taskQueue.QueueBackgroundWorkItemAsync(async ct =>
        {
            using var scope = _serviceProvider.CreateScope();
            var emailService = scope.ServiceProvider.GetRequiredService<IAccountNotificationSender>();
            await emailService.SendEmailForRegisterConfirmationAsync(user, token, scheme, host, ct);
        });

        return true;
    }

    public async Task<bool> ForgotPasswordAsync(string email, string scheme, string host)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(email);

        if (user is null)
        {
            throw new BadRequestException("If the email address was registered, you will receive an email with a link to restore it.");
        }

        var token = await _unitOfWork.Auth.GeneratePasswordResetTokenAsync(user);

        user.IsPasswordReset = true;
        await _unitOfWork.Users.UpdateAsync(user);

        await _taskQueue.QueueBackgroundWorkItemAsync(async ct =>
        {
            using var scope = _serviceProvider.CreateScope();
            var emailService = scope.ServiceProvider.GetRequiredService<IAccountNotificationSender>();
            await emailService.SendEmailForResetPasswordAsync(user, token, scheme, host, ct);
        });

        return true;
    }

    public async Task<IdentityResultDto> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
    {
        if (resetPasswordDto.NewPassword != resetPasswordDto.ConfirmPassword)
        {
            return new IdentityResultDto
            {
                Succeeded = false,
                Errors = ["Passwords do not match."]
            };
        }

        var user = await _unitOfWork.Users.GetByEmailAsync(resetPasswordDto.Email);

        if (user is null)
        {
            return new IdentityResultDto
            {
                Succeeded = true,
            };
        }

        var decodedToken = WebEncoders.Base64UrlDecode(resetPasswordDto.Token);
        var normalizedToken = Encoding.UTF8.GetString(decodedToken);

        var result = await _unitOfWork.Auth.ResetPasswordAsync(user, normalizedToken, resetPasswordDto.NewPassword);

        if (result.Succeeded)
        {
            user.IsPasswordReset = false;
            await _unitOfWork.Users.UpdateAsync(user);
        }

        return new IdentityResultDto
        {
            Succeeded = result.Succeeded,
            Errors = result.Errors.Select(e => e.Description)
        };
    }

    public async Task<LoginResultDto> LoginAsync(UserLoginDto userLoginDto)
    {
        if (string.IsNullOrEmpty(userLoginDto.Email) || string.IsNullOrEmpty(userLoginDto.Password))
        {
            throw new BadRequestException("Email and password are required.");
        }

        var user = await _unitOfWork.Users.GetByEmailAsync(userLoginDto.Email);

        if (user is null)
        {
            return new LoginResultDto
            {
                Succeeded = false,
                FailureReason = LoginFailureReason.InvalidCredentials,
                Message = "Invalid email or password."
            };
        }

        if (!user.EmailConfirmed)
        {
            return new LoginResultDto
            {
                Succeeded = false,
                FailureReason = LoginFailureReason.EmailNotConfirmed,
                Message = "Please confirm your email address before logging in."
            };
        }

        if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow)
        {
            return new LoginResultDto
            {
                Succeeded = false,
                FailureReason = LoginFailureReason.AccountLocked,
                Message = $"Your account has been locked until {user.LockoutEnd.Value:yyyy-MM-dd HH:mm} UTC. Please contact support."
            };
        }
        
        if (user.IsPasswordReset)
        {
            return new LoginResultDto
            {
                Succeeded = false,
                FailureReason = LoginFailureReason.PasswordResetRequired,
                Message = "Your password has been reset. Please set a new password."
            };
        }

        var signInResult = await _unitOfWork.Auth.CheckPasswordAsync(user, userLoginDto.Password);

        if (!signInResult)
        {
            return new LoginResultDto
            {
                Succeeded = false,
                FailureReason = LoginFailureReason.InvalidCredentials,
                Message = "Invalid email or password."
            };
        }

        var claims = await _claimsService.GenerateUserClaimsAsync(user.Id, user.Email);
        var token = _jwtTokenProvider.GenerateToken(claims);
        var refreshToken = _jwtTokenProvider.GenerateRefreshToken();

        var refreshTokenEntity = new RefreshTokenModel
        {
            Token = refreshToken,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtTokenProvider.GetRefreshTokenExpirationInDays()),
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.RefreshTokens.AddAsync(refreshTokenEntity);

        return new LoginResultDto
        {
            Succeeded = true,
            UserId = user.Id,
            Token = token,
            RefreshToken = refreshToken,
            Message = "Login successful."
        };
    }

    public async Task<LoginResultDto> RefreshTokenAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new BadRequestException("Refresh token is required.");
        }

        var tokenEntity = await _unitOfWork.RefreshTokens.GetByTokenAsync(refreshToken);

        if (tokenEntity is null || !tokenEntity.IsActive)
        {
            return new LoginResultDto
            {
                Succeeded = false,
                Message = "Invalid or expired refresh token."
            };
        }

        var user = tokenEntity.User;
        
        if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow)
        {
            
            tokenEntity.IsRevoked = true;
            tokenEntity.RevokedAt = DateTime.UtcNow;
            await _unitOfWork.RefreshTokens.UpdateAsync(tokenEntity);
        
            return new LoginResultDto
            {
                Succeeded = false,
                FailureReason = LoginFailureReason.AccountLocked,
                Message = $"Your account has been locked until {user.LockoutEnd.Value:yyyy-MM-dd HH:mm} UTC. Please contact support."
            };
        }

        //  Revoke the old refresh token
        tokenEntity.IsRevoked = true;
        tokenEntity.RevokedAt = DateTime.UtcNow;
        await _unitOfWork.RefreshTokens.UpdateAsync(tokenEntity);

        // Generate new JWT and refresh token
        var claims = await _claimsService.GenerateUserClaimsAsync(user.Id, user.Email);
        var newAccessToken = _jwtTokenProvider.GenerateToken(claims);
        var newRefreshToken = _jwtTokenProvider.GenerateRefreshToken();

        var newRefreshTokenEntity = new RefreshTokenModel
        {
            Token = newRefreshToken,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtTokenProvider.GetRefreshTokenExpirationInDays()),
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.RefreshTokens.AddAsync(newRefreshTokenEntity);

        return new LoginResultDto
        {
            Succeeded = true,
            Token = newAccessToken,
            RefreshToken = newRefreshToken,
            Message = "Token refreshed successfully."
        };
    }

    public async Task LogoutAsync(string refreshToken)
    {
        var tokenEntity = await _unitOfWork.RefreshTokens.GetByTokenAsync(refreshToken);

        if (tokenEntity is null)
        {
            return; 
        }

        await _unitOfWork.RefreshTokens.RevokeAllUserTokensAsync(tokenEntity.UserId);
        await _unitOfWork.Auth.LogoutAsync();
    }

    public async Task<IdentityResultDto> ChangePasswordAsync(ChangePasswordDto changePasswordDto)
    {
        if (changePasswordDto.NewPassword != changePasswordDto.ConfirmPassword)
        {
            return new IdentityResultDto
            {
                Succeeded = false,
                Errors = ["Passwords do not match."]
            };
        }
        var user = await _unitOfWork.Users.GetByIdAsync(changePasswordDto.UserId);
        if (user is null)
        {
            return new IdentityResultDto
            {
                Succeeded = false,
                Errors = ["User not found."]
            };
        }

        var isCurrentPasswordValid = await _unitOfWork.Auth.CheckPasswordAsync(user, changePasswordDto.CurrentPassword);

        if (!isCurrentPasswordValid)
        {
            return new IdentityResultDto
            {
                Succeeded = false,
                Errors = ["Current password is incorrect."]
            };
        }

        var result = await _unitOfWork.Auth.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);

        return new IdentityResultDto
        {
            Succeeded = result.Succeeded,
            Errors = result.Errors.Select(e => e.Description)
        };
    }
}
