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

    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        return await _dbContext.RefreshTokens
            .Include(rt => rt.User) 
            .FirstOrDefaultAsync(rt => rt.Token == token);
    } 

    public async Task AddAsync(RefreshToken refreshToken)
    {
        await _dbContext.RefreshTokens.AddAsync(refreshToken);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(RefreshToken refreshToken)
    {
        _dbContext.RefreshTokens.Update(refreshToken);
        await _dbContext.SaveChangesAsync();
    }

    public async Task RevokeAllUserTokensAsync(string userId)
    {
        await _dbContext.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow)
            .ExecuteUpdateAsync(setters => setters
            .SetProperty(rt => rt.IsRevoked, true)
            .SetProperty(rt => rt.RevokedAt, DateTime.UtcNow));
    }

    public async Task RemoveAsync()
    {
        await _dbContext.RefreshTokens
            .Where(rt => rt.ExpiresAt <= DateTime.UtcNow || rt.IsRevoked)
            .ExecuteDeleteAsync();
    }
}
