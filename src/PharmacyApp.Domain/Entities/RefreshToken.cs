using System.ComponentModel.DataAnnotations;

namespace PharmacyApp.Domain.Entities;

public class RefreshToken
{
    [Key]
    public int Id { get; private set; }
    [Required]
    public string Token { get; private set; } = string.Empty;
    [Required]
    public string UserId { get; private set; } = string.Empty;
    public User? User { get; set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; private set; }
    public bool IsRevoked { get; private set; } = false;
    public DateTime? RevokedAt { get; private set; }
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => !IsRevoked && !IsExpired;
    
    private RefreshToken() { }

    public RefreshToken(string token, string userId, DateTime expiresAt)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token cannot be empty.");
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("UserId cannot be empty.");
        if (expiresAt <= DateTime.UtcNow)
            throw new ArgumentException("Expiration date must be in the future.");

        Token = token;
        UserId = userId;
        ExpiresAt = expiresAt;
        CreatedAt = DateTime.UtcNow;
        IsRevoked = false;
    }

    public void Revoke()
    {
        if (IsRevoked)
            throw new InvalidOperationException("Token is already revoked.");
        
        IsRevoked = true;
        RevokedAt = DateTime.UtcNow;
    }
}
