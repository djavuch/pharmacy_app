namespace PharmacyApp.Domain.Entities;

public class Wishlist
{
    public string UserId { get; set; } 
    public User User { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; }   
    public DateTime DateAdded { get; set; } = DateTime.UtcNow;
}
