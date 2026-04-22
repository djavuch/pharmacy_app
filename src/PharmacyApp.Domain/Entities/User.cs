using Microsoft.AspNetCore.Identity;
using PharmacyApp.Domain.Entities.Bonus;

namespace PharmacyApp.Domain.Entities;

public class User : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<UserAddress> Addresses { get; set; } = [];
    public ICollection<Order> Orders { get; set; } = [];
    public ICollection<Review> Reviews { get; set; } = [];
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
    public ICollection<Wishlist> Wishlist { get; set; } = [];
    public BonusAccount? BonusAccount { get; set; }

    public void UpdateProfile(
        string? firstName,
        string? lastName,
        string? phoneNumber)
    {
        if (firstName is not null)
            FirstName = NormalizeRequired(firstName, nameof(FirstName), 50);

        if (lastName is not null)
            LastName = NormalizeRequired(lastName, nameof(LastName), 50);

        if (phoneNumber is not null)
            PhoneNumber = NormalizeOptional(phoneNumber, 25);

        UpdatedAt = DateTime.UtcNow;
    }

    private static string NormalizeRequired(string value, string fieldName, int maxLength)
    {
        var normalized = value.Trim();

        if (string.IsNullOrWhiteSpace(normalized))
            throw new ArgumentException($"{fieldName} cannot be empty.");

        if (normalized.Length > maxLength)
            throw new ArgumentException($"{fieldName} cannot exceed {maxLength} characters.");

        return normalized;
    }

    private static string? NormalizeOptional(string value, int maxLength)
    {
        var normalized = value.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
            return null;

        if (normalized.Length > maxLength)
            throw new ArgumentException($"Value cannot exceed {maxLength} characters.");

        return normalized;
    }
}
