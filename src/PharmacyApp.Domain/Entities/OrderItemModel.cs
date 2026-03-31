namespace PharmacyApp.Domain.Entities;

public class OrderItemModel
{
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public string? ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    
    public decimal? DiscountAmount { get; set; }
    public Guid? AppliedDiscountId { get; set; } // Id of the applied discount

    public decimal Subtotal => Price * Quantity;
    public decimal Total => Subtotal - (DiscountAmount ?? 0);

    public OrderModel? Order { get; set; }
    public ProductModel? Product { get; set; }
}
