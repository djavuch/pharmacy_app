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

    public async Task<SignInResult> LoginAsync(UserModel user, string password, bool rememberMe)
    {
        return await _signInManager.PasswordSignInAsync(
            user,
            password,
            isPersistent: rememberMe,
            lockoutOnFailure: false
            );
    }

    public async Task<bool> CheckPasswordAsync(UserModel user, string password)
    {
        return await _userManager.CheckPasswordAsync(user, password);
    }

    public async Task UnlockUserAsync(UserModel user)
    {
        await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow);
        await _userManager.ResetAccessFailedCountAsync(user);
    }

    public async Task LogoutAsync()
    {
        await _signInManager.SignOutAsync();
    }

    public async Task<string> GenerateEmailConfirmationTokenAsync(UserModel user)
    {
        _logger.LogWarning("=== DIAGNOSTICS: GenerateEmailConfirmationTokenAsync ===");
        _logger.LogWarning("User ID: {UserId}, Email: {Email}", user.Id, user.Email);

        try
        {
            // Проверяем все зарегистрированные токен-провайдеры
            var tokenProviders = _userManager.Options.Tokens.ProviderMap;
            _logger.LogWarning("Total token providers registered: {Count}", tokenProviders.Count);

            foreach (var provider in tokenProviders)
            {
                _logger.LogWarning("Provider: {Key} -> {Type}", provider.Key, provider.Value.ProviderType.Name);
            }

            // Проверяем, какой провайдер используется для email confirmation
            var emailProvider = _userManager.Options.Tokens.EmailConfirmationTokenProvider;
            _logger.LogWarning("Email confirmation token provider name: {Provider}", emailProvider);

            if (tokenProviders.TryGetValue(emailProvider, out var providerInfo))
            {
                _logger.LogWarning("Email provider type: {Type}", providerInfo.ProviderType.FullName);
                _logger.LogWarning("Email provider instance type: {Instance}", providerInfo.ProviderInstance?.GetType().FullName ?? "NULL");
            }
            else
            {
                _logger.LogError("Email provider '{Provider}' NOT FOUND in ProviderMap!", emailProvider);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking token providers");
        }

        // Пробуем получить провайдер напрямую
        try
        {
            var actualProvider = await _userManager.GetValidTwoFactorProvidersAsync(user);
            _logger.LogWarning("Valid 2FA providers for user: {Providers}", string.Join(", ", actualProvider));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get valid 2FA providers");
        }

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        _logger.LogWarning("=== Token Generated ===");
        _logger.LogWarning("Token length: {Length}", token.Length);
        _logger.LogWarning("Token content: {Token}", token);
        _logger.LogWarning("Token is numeric: {IsNumeric}", long.TryParse(token, out _));

        return token;
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

    public async Task<IList<string>> GetRolesAsync(UserModel user)
    {
        return await _userManager.GetRolesAsync(user);
    }
}
