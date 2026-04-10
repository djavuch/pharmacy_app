namespace PharmacyApp.Application.Contracts.Wishlist.Admin;

public record WishlistUserDto
{
    public string UserId { get; set; }
    public string UserEmail { get; set; }
    public string UserFullName { get; set; }
    public DateTime DateAdded { get; set; }
}