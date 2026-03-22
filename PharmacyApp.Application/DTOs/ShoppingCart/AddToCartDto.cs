namespace PharmacyApp.Application.DTOs.ShoppingCart;

public record AddToCartDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
