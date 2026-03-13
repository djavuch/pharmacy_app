namespace PharmacyApp.Application.DTOs.Order;

public class OrderItemResponseDto
{
    public int OrderId { get; set; }
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}
