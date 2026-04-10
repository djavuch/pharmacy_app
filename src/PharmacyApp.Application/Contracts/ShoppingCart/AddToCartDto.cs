namespace PharmacyApp.Application.Contracts.ShoppingCart;

public record AddToCartDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
