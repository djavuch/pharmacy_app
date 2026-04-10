namespace PharmacyApp.Domain.Entities;

public class OrderItem
{
    public int OrderId { get; private set; }
    public int ProductId { get; private set; }
    public string? ProductName { get; private set; }
    public int Quantity { get; private set; }
    public decimal Price { get; private set; }
    
    public decimal? DiscountAmount { get; private set; }
    public Guid? AppliedDiscountId { get; private set; }

    public Order? Order { get; set; }
    public Product? Product { get; set; }
    
    private OrderItem() { }

    public OrderItem(int productId, string productName, int quantity, decimal price)
    {
        if (productId <= 0)
            throw new ArgumentException("ProductId must be greater than 0.");
        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("Product name cannot be empty.");
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than 0.");
        if (price <= 0)
            throw new ArgumentException("Price must be greater than 0.");
        
        ProductId = productId;
        ProductName = productName;
        Quantity = quantity;
        Price = price;
    }
    
    public decimal Subtotal => Price * Quantity;
    public decimal Total => Subtotal - (DiscountAmount ?? 0);
}
