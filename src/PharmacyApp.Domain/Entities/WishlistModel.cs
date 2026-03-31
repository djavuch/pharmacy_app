namespace PharmacyApp.Domain.Entities;

public class WishlistModel
{
    public string UserId { get; set; } 
    public UserModel User { get; set; }
    public int ProductId { get; set; }
    public ProductModel Product { get; set; }   
    public DateTime DateAdded { get; set; } = DateTime.UtcNow;
}
