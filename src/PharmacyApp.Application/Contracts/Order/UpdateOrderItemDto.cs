namespace PharmacyApp.Application.Contracts.Order;
public record UpdateOrderItemDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
