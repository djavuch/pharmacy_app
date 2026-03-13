using System.ComponentModel.DataAnnotations;

namespace PharmacyApp.Domain.Entities;

public class RefreshTokenModel
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string Token { get; set; } = string.Empty;
    [Required]
    public string UserId { get; set; } = string.Empty;
    public UserModel? User { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; } = false;
    public DateTime? RevokedAt { get; set; }
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => !IsRevoked && !IsExpired;
}
