using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PharmacyApp.Application.Interfaces.Services; 
using PharmacyApp.Application.Mappers;
using PharmacyApp.Domain.Entities;
using System.Text;
using PharmacyApp.Application.Contracts.User.Account;
using PharmacyApp.Application.Contracts.User.Profile;
using PharmacyApp.Application.Contracts.User.Results;
using PharmacyApp.Application.Interfaces.Abstractions;
using PharmacyApp.Application.Interfaces.Abstractions.Authentication;
using PharmacyApp.Application.Interfaces.Email;
using PharmacyApp.Application.Interfaces.Repositories;
using PharmacyApp.Domain.Common;

namespace PharmacyApp.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWorkRepository _unitOfWork;
    private readonly IJwtTokenProvider _jwtTokenProvider;
    private readonly IClaimsService _claimsService;
    private readonly IBackgroundTaskQueue _taskQueue;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<AuthService> _logger;


    public AuthService(IUnitOfWorkRepository unitOfWork, IJwtTokenProvider jwtTokenProvider, 
        IClaimsService claimsService, IBackgroundTaskQueue taskQueue, 
        IServiceScopeFactory serviceScopeFactory,
        ILogger<AuthService> logger)
    {
        _unitOfWork = unitOfWork;
        _jwtTokenProvider = jwtTokenProvider;
        _claimsService = claimsService;
        _taskQueue = taskQueue;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    public async Task<Result<UserProfileDto>> UserRegisterAsync(UserRegistrationDto userRegistrationDto, string scheme, string host)
    {
        var existingUser = await _unitOfWork.Users.GetByEmailAsync(userRegistrationDto.Email);

        if (existingUser is not null)
        {
            return Result<UserProfileDto>.Conflict("User with this email already exists.");
        }
        
        var newUser = new User
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
            return Result<UserProfileDto>
                .BadRequest("User registration failed: "
                            + string.Join(", ", result.Errors.Select(e => e.Description)));
        }
        
        await _unitOfWork.Auth.AddToRoleAsync(newUser, "Customer");
        

        var token = await _unitOfWork.Auth.GenerateEmailConfirmationTokenAsync(newUser);

        _logger.LogInformation("Queueing email confirmation message for user {UserId}.", newUser.Id);

        await _taskQueue.QueueBackgroundWorkItemAsync(async ct =>
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var emailService = scope.ServiceProvider.GetRequiredService<IAccountNotificationSender>();
            await emailService.SendEmailForRegisterConfirmationAsync(newUser, token, scheme, host, ct);
        });

        return Result<UserProfileDto>.Success(newUser.ToUserDto("Customer")); 
    }

    public async Task<Result<bool>> ConfirmEmailAsync(string userId, string token)
    {
        if (userId is null || string.IsNullOrEmpty(token))
        {
            return Result<bool>.BadRequest("User ID and token are required for email confirmation.");
        }

        var user = await _unitOfWork.Users.GetByIdAsync(userId);

        if (user is null)
        {
            return Result<bool>.NotFound("User not found for email confirmation.");
        }

        string normalizedToken;
        try
        {
            var decodedToken = WebEncoders.Base64UrlDecode(token);
            normalizedToken = Encoding.UTF8.GetString(decodedToken);
        }
        catch (Exception ex) when (ex is FormatException or ArgumentException)
        {
            return Result<bool>.BadRequest("Invalid email confirmation token.");
        }

        var result = await _unitOfWork.Auth.ConfirmEmailAsync(user, normalizedToken);

        if (!result.Succeeded)
        {
            return Result<bool>.BadRequest(
                string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> ResendConfirmationEmailAsync(string email, string scheme, string host)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(email);

        if (user is null || user.EmailConfirmed)
        {
            return Result<bool>.NotFound("User not found for email confirmation.");
        }

        var stampUpdateResult = await _unitOfWork.Auth.UpdateSecurityStampAsync(user);
        if (!stampUpdateResult.Succeeded)
        {
            return Result<bool>
                .BadRequest("Failed to regenerate confirmation token: " + 
                            string.Join(", ", stampUpdateResult.Errors.Select(e => e.Description)));
        }
        
        var token = await _unitOfWork.Auth.GenerateEmailConfirmationTokenAsync(user);

        _logger.LogInformation("Queueing resent email confirmation message for user {UserId}.", user.Id);

        await _taskQueue.QueueBackgroundWorkItemAsync(async ct =>
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var emailService = scope.ServiceProvider.GetRequiredService<IAccountNotificationSender>();
            await emailService.SendEmailForRegisterConfirmationAsync(user, token, scheme, host, ct);
        });

        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> ForgotPasswordAsync(string email, string scheme, string host)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(email);

        if (user is null)
        {
            _logger.LogInformation("Password reset requested for non-existing email.");
            return Result<bool>.Success(true, "If the email address was registered, you will receive an email with a link to restore it.");
        }

        var token = await _unitOfWork.Auth.GeneratePasswordResetTokenAsync(user);

        _logger.LogInformation("Queueing password reset message for user {UserId}.", user.Id);

        await _taskQueue.QueueBackgroundWorkItemAsync(async ct =>
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var emailService = scope.ServiceProvider.GetRequiredService<IAccountNotificationSender>();
            await emailService.SendEmailForResetPasswordAsync(user, token, scheme, host, ct);
        });

        return Result<bool>.Success(true, "If the email address was registered, you will receive an email.");
    }

    public async Task<IdentityOperationResult> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
    {
        if (resetPasswordDto.NewPassword != resetPasswordDto.ConfirmPassword)
        {
            return new IdentityOperationResult
            {
                Succeeded = false,
                Errors = ["Passwords do not match."]
            };
        }

        var user = await _unitOfWork.Users.GetByEmailAsync(resetPasswordDto.Email);

        if (user is null)
        {
            return new IdentityOperationResult
            {
                Succeeded = true,
            };
        }

        var decodedToken = WebEncoders.Base64UrlDecode(resetPasswordDto.Token);
        var normalizedToken = Encoding.UTF8.GetString(decodedToken);

        var result = await _unitOfWork.Auth.ResetPasswordAsync(user, normalizedToken, resetPasswordDto.NewPassword);

        return new IdentityOperationResult
        {
            Succeeded = result.Succeeded,
            Errors = result.Errors.Select(e => e.Description)
        };
    }

    public async Task<LoginResult> LoginAsync(UserLoginDto userLoginDto)
    {
        if (string.IsNullOrEmpty(userLoginDto.Email) || string.IsNullOrEmpty(userLoginDto.Password))
        {
            return new LoginResult
            {
                Succeeded = false,
                FailureReason = LoginFailureReason.InvalidCredentials,
                Message = "Email and password are required."
            };
        }

        var user = await _unitOfWork.Users.GetByEmailAsync(userLoginDto.Email);

        if (user is null)
        {
            return new LoginResult
            {
                Succeeded = false,
                FailureReason = LoginFailureReason.InvalidCredentials,
                Message = "Invalid email or password."
            };
        }

        if (!user.EmailConfirmed)
        {
            return new LoginResult
            {
                Succeeded = false,
                FailureReason = LoginFailureReason.EmailNotConfirmed,
                Message = "Please confirm your email address before logging in."
            };
        }

        if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow)
        {
            return new LoginResult
            {
                Succeeded = false,
                FailureReason = LoginFailureReason.AccountLocked,
                Message = $"Your account has been locked until {user.LockoutEnd.Value:yyyy-MM-dd HH:mm} UTC. Please contact support."
            };
        }
        
        var signInResult = await _unitOfWork.Auth
            .CheckPasswordForSignInAsync(user, userLoginDto.Password, lockoutOnFailure: true);

        if (signInResult.IsLockedOut)
        {
            var lockedUser = await _unitOfWork.Users.GetByIdAsync(user.Id);
            var lockoutEnd = lockedUser?.LockoutEnd ?? user.LockoutEnd;
            var lockoutMessage = lockoutEnd.HasValue
                ? $"Your account has been locked until {lockoutEnd.Value:yyyy-MM-dd HH:mm} UTC. Please contact support."
                : "Your account has been locked. Please contact support.";

            return new LoginResult
            {
                Succeeded = false,
                FailureReason = LoginFailureReason.AccountLocked,
                Message = lockoutMessage,
            };
        }

        if (!signInResult.Succeeded)
        {
            return new LoginResult
            {
                Succeeded = false,
                FailureReason = LoginFailureReason.InvalidCredentials,
                Message = "Invalid email or password."
            };
        }

        var claims = await _claimsService.GenerateUserClaimsAsync(user.Id, user.Email);
        var token = _jwtTokenProvider.GenerateToken(claims);
        var refreshToken = _jwtTokenProvider.GenerateRefreshToken();
        
        var refreshTokenEntity = new RefreshToken(refreshToken, user.Id, DateTime.UtcNow.AddDays(_jwtTokenProvider.GetRefreshTokenExpirationInDays()));
        
        await _unitOfWork.RefreshTokens.AddAsync(refreshTokenEntity);
        await _unitOfWork.SaveChangesAsync();

        return new LoginResult
        {
            Succeeded = true,
            UserId = user.Id,
            Token = token,
            RefreshToken = refreshToken,
            Message = "Login successful."
        };
    }

    public async Task<LoginResult> RefreshTokenAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return new LoginResult
            {
                Succeeded = false,
                Message = "Refresh token is required."
            };
        }

        var tokenEntity = await _unitOfWork.RefreshTokens.GetByTokenAsync(refreshToken);

        if (tokenEntity is null || !tokenEntity.IsActive)
        {
            return new LoginResult
            {
                Succeeded = false,
                Message = "Invalid or expired refresh token."
            };
        }

        var user = tokenEntity.User;
        
        if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow)
        {
            tokenEntity.Revoke();
            await _unitOfWork.RefreshTokens.UpdateAsync(tokenEntity);
            await _unitOfWork.SaveChangesAsync();
        
            return new LoginResult
            {
                Succeeded = false,
                FailureReason = LoginFailureReason.AccountLocked,
                Message = $"Your account has been locked until {user.LockoutEnd.Value:yyyy-MM-dd HH:mm} UTC. Please contact support."
            };
        }

        //  Revoke the old refresh token
        tokenEntity.Revoke();
        await _unitOfWork.RefreshTokens.UpdateAsync(tokenEntity);

        // Generate new JWT and refresh token
        var claims = await _claimsService.GenerateUserClaimsAsync(user.Id, user.Email);
        var newAccessToken = _jwtTokenProvider.GenerateToken(claims);
        var newRefreshToken = _jwtTokenProvider.GenerateRefreshToken();
        
        var newRefreshTokenEntity = new RefreshToken(newRefreshToken, user.Id, DateTime.UtcNow.AddDays(_jwtTokenProvider.GetRefreshTokenExpirationInDays()));
        
        await _unitOfWork.RefreshTokens.AddAsync(newRefreshTokenEntity);
        await _unitOfWork.SaveChangesAsync();

        return new LoginResult
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

    public async Task<IdentityOperationResult> ChangePasswordAsync(ChangePasswordDto changePasswordDto)
    {
        if (changePasswordDto.NewPassword != changePasswordDto.ConfirmPassword)
        {
            return new IdentityOperationResult
            {
                Succeeded = false,
                Errors = ["Passwords do not match."]
            };
        }
        var user = await _unitOfWork.Users.GetByIdAsync(changePasswordDto.UserId);
        if (user is null)
        {
            return new IdentityOperationResult
            {
                Succeeded = false,
                Errors = ["User not found."]
            };
        }

        var isCurrentPasswordValid = await _unitOfWork.Auth.CheckPasswordAsync(user, changePasswordDto.CurrentPassword);

        if (!isCurrentPasswordValid)
        {
            return new IdentityOperationResult
            {
                Succeeded = false,
                Errors = ["Current password is incorrect."]
            };
        }

        var result = await _unitOfWork.Auth.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);

        return new IdentityOperationResult
        {
            Succeeded = result.Succeeded,
            Errors = result.Errors.Select(e => e.Description)
        };
    }
}
