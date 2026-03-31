using System.Security.Claims;

namespace PharmacyApp.Infrastructure.Abstractions.Authentication;

public interface IClaimsService
{
    Task<IEnumerable<Claim>> GenerateUserClaimsAsync(string userId, string email);
}