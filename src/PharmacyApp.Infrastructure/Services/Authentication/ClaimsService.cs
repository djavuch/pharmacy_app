using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using PharmacyApp.Domain.Entities;
using PharmacyApp.Infrastructure.Abstractions.Authentication;
using static PharmacyApp.Domain.Exceptions.AppExceptions;

namespace PharmacyApp.Infrastructure.Services.Authentication;

public class ClaimsService : IClaimsService
{
    private readonly UserManager<UserModel> _userManager;

    public ClaimsService(UserManager<UserModel> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IEnumerable<Claim>> GenerateUserClaimsAsync(string userId, string email)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
        {
            throw new BadRequestException("User not found.");
        }
        
        var roles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat,
                new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }
        
        return claims;
    }
}