namespace PharmacyApp.Application.DTOs.Order;

internal record OrderItemDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}