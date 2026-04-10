namespace PharmacyApp.Domain.Entities;

public class CartItem
{
    public int CartId { get; private set; }
    public ShoppingCart ShoppingCart { get; set; }
    public int ProductId { get; private set; }
    public Product? Product { get; set; }
    public int Quantity { get; private set; }
    public decimal PriceAtAdd { get; private set; }
    public DateTime AddedAt { get; private set; } = DateTime.UtcNow;
    
    private CartItem() { }

    public CartItem(int cartId, int productId, int quantity, decimal priceAtAdd)
    {
        if (cartId <= 0)
            throw new ArgumentException("CartId must be greater than 0.");
        if (productId <= 0)
            throw new ArgumentException("ProductId must be greater than 0.");
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than 0.");
        if (priceAtAdd <= 0)
            throw new ArgumentException("Price must be greater than 0.");

        CartId = cartId;
        ProductId = productId;
        Quantity = quantity;
        PriceAtAdd = priceAtAdd;
        AddedAt = DateTime.UtcNow;
    }
    
    public void AddQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity to add must be greater than 0.");
        Quantity += quantity;
    }
    
    public void SetQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than 0.");
        Quantity = quantity;
    }
}
