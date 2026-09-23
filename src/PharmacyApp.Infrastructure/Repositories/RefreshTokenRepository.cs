using Microsoft.EntityFrameworkCore;
using PharmacyApp.Application.Interfaces.RefreshTokens;
using PharmacyApp.Domain.Entities;
using PharmacyApp.Infrastructure.Data;

namespace PharmacyApp.Infrastructure.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    public readonly PharmacyAppDbContext _dbContext;

    public RefreshTokenRepository(PharmacyAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default)
    {
        return await _dbContext.RefreshTokens
            .Include(rt => rt.User) 
            .FirstOrDefaultAsync(rt => rt.Token == token, ct);
    } 

    public async Task AddAsync(RefreshToken refreshToken, CancellationToken ct = default)
    {
        await _dbContext.RefreshTokens.AddAsync(refreshToken, ct);
    }

    public Task UpdateAsync(RefreshToken refreshToken, CancellationToken ct = default)
    {
        _dbContext.RefreshTokens.Update(refreshToken);
        return Task.CompletedTask;
    }

    public async Task RevokeAllUserTokensAsync(string userId, CancellationToken ct = default)
    {
        await _dbContext.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow)
            .ExecuteUpdateAsync(setters => setters
            .SetProperty(rt => rt.IsRevoked, true)
            .SetProperty(rt => rt.RevokedAt, DateTime.UtcNow), ct);
    }

    public async Task RemoveAsync(CancellationToken ct = default)
    {
        await _dbContext.RefreshTokens
            .Where(rt => rt.ExpiresAt <= DateTime.UtcNow || rt.IsRevoked)
            .ExecuteDeleteAsync(ct);
    }
}
