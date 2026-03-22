namespace PharmacyApp.Application.DTOs.Order;
public record UpdateOrderDto
{
    public int OrderId { get; set; }
    public OrderAddressDto ShippingAddress { get; set; }
    public List<UpdateOrderItemDto> OrderItems { get; set; } = [];
}
