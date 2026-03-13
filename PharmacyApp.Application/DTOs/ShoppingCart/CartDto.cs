namespace PharmacyApp.Application.DTOs.ShoppingCart;

public class CartDto
{
    public int Id { get; set; }
    public string? UserId { get; set; }
    public List<CartItemDto> Items { get; set; } = [];
    public decimal TotalPrice { get; set; }
    public DateTime LastModifiedAt { get; set; }
}
