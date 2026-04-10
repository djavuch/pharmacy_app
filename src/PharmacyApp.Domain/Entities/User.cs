using Microsoft.AspNetCore.Identity;
using PharmacyApp.Domain.Entities.Bonus;
using System.ComponentModel.DataAnnotations;

namespace PharmacyApp.Domain.Entities;

public class User : IdentityUser 
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Address { get; set; }
    public DateTime DateOfBirth { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsPasswordReset { get; set; } = false;
    public List<UserAddress> Addresses { get; set; } = [];
    public ICollection<Order> Orders { get; set; } = [];
    public ICollection<Review> Reviews { get; set; } = [];
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
    public ICollection<Wishlist> Wishlist { get; set; } = [];
    public BonusAccount? BonusAccount { get; set; }
}