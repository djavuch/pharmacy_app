using System.Security.Claims;

namespace PharmacyApp.Application.Interfaces.Abstractions.Authentication;

public interface IClaimsService
{
    Task<IEnumerable<Claim>> GenerateUserClaimsAsync(string userId, string email);
}