namespace PharmacyApp.Application.Contracts.Order;

public record OrderItemResponseDto
{
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Subtotal { get; set; }
}
