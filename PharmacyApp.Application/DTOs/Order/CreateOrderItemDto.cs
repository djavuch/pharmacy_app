namespace PharmacyApp.Application.DTOs.Order;

public class CreateOrderItemDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public int Quantity { get; set; }
}