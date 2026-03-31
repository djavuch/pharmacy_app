namespace PharmacyApp.Application.DTOs.ShoppingCart;

public record UpdateCartDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
