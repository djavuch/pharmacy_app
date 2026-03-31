namespace PharmacyApp.Application.DTOs.Order;
public record UpdateOrderItemDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
