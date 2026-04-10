namespace PharmacyApp.Application.Contracts.ShoppingCart;

public record UpdateCartDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
