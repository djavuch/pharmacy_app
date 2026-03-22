namespace PharmacyApp.Application.DTOs.ShoppingCart;

public record CartItemDto
{
    public int CartId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Subtotal { get; set; }
    public int AvailableStock { get; set; }
}
