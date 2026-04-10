namespace PharmacyApp.Domain.Entities;

public class ShoppingCart
{
    public int Id { get; private set; }
    public string? UserId { get; private set; }
    public User? User { get; set; }
    public string? SessionId { get; private set; }  
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public ICollection<CartItem> Items { get; set; } = [];
    
    public decimal GetTotalAmount()
    {
        return Items?.Sum(item => item.Quantity * item.PriceAtAdd) ?? 0;
    }
    
    public void UpdateTimestamp()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    public ShoppingCart(string userId, string? sessionId)
    {
        if (string.IsNullOrEmpty(userId) && string.IsNullOrEmpty(sessionId))
            throw new ArgumentException("Either userId or sessionId must be provided.");
        
        UserId = userId;
        SessionId = sessionId;
        CreatedAt = DateTime.UtcNow;
        Items = [];
    }
    
    public void AssignToUser(string userId)
    {
        UserId = userId;
        SessionId = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AssignToGuest(string sessionId)
    {
        SessionId = sessionId;
        UpdatedAt = DateTime.UtcNow;
    }
}
