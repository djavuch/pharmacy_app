using PharmacyApp.Domain.Entities;

namespace PharmacyApp.Application.Interfaces.RefreshTokens;

public interface IRefreshTokenRepository
{
    Task<RefreshTokenModel?> GetByTokenAsync(string token);
    Task AddAsync(RefreshTokenModel refreshToken);
    Task UpdateAsync(RefreshTokenModel refreshToken);
    Task RevokeAllUserTokensAsync(string userId);
    Task RemoveAsync();
}
