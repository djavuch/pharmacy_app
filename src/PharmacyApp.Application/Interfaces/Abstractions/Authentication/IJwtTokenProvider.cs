using System.Security.Claims;

namespace PharmacyApp.Application.Interfaces.Abstractions.Authentication;

public interface IJwtTokenProvider
{
    string GenerateToken(IEnumerable<Claim> claims);
    string GenerateRefreshToken();
    int GetRefreshTokenExpirationInDays();
}