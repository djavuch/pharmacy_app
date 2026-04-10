using PharmacyApp.Domain.Entities;

namespace PharmacyApp.Application.Interfaces.RefreshTokens;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task AddAsync(RefreshToken refreshToken);
    Task UpdateAsync(RefreshToken refreshToken);
    Task RevokeAllUserTokensAsync(string userId);
    Task RemoveAsync();
}
