namespace PharmacyApp.Domain.Entities;

public class ShoppingCartModel
{
    public int Id { get; set; }
    public string? UserId { get; set; }
    public UserModel? User { get; set; }
    public string? SessionId { get; set; }  // for unregistered users
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<CartItemModel>? Items { get; set; }
    public decimal GetTotalAmount()
    {
        return Items?.Sum(item => item.Quantity * item.PriceAtAdd) ?? 0;
    }
    public void UpdateTimestamp()
    {
        UpdatedAt = DateTime.UtcNow;
    }   
}
