namespace PharmacyApp.Domain.Entities;

public class CartItemModel
{
    public int CartId { get; set; }
    public ShoppingCartModel ShoppingCart { get; set; }
    public int ProductId { get; set; }
    public ProductModel? Product { get; set; }
    public int Quantity { get; set; }
    public decimal PriceAtAdd { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}
