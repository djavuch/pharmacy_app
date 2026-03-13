namespace PharmacyApp.Application.Interfaces.JWT;

public interface IJwtService
{
    Task<string> GenerateToken(string userId, string userName);
    string GenerateRefreshToken();
    int GetRefreshTokenExpirationInDays();
}
