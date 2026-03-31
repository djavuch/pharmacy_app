using Microsoft.AspNetCore.Identity;
using PharmacyApp.Domain.Entities.Bonus;
using System.ComponentModel.DataAnnotations;

namespace PharmacyApp.Domain.Entities;

public class UserModel : IdentityUser 
{
    [Required]
    [MaxLength(100)]
    public string? FirstName { get; set; }
    [Required]
    [MaxLength(100)]
    public string? LastName { get; set; }
    public string? Address { get; set; }
    public DateTime DateOfBirth { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsPasswordReset { get; set; } = false;
    public List<UserAddressModel> Addresses { get; set; } = [];
    public ICollection<OrderModel> Orders { get; set; } = [];
    public ICollection<ReviewModel> Reviews { get; set; } = [];
    public ICollection<RefreshTokenModel> RefreshTokens { get; set; } = [];
    public ICollection<WishlistModel> Wishlist { get; set; } = [];
    public BonusAccountModel? BonusAccount { get; set; }
}