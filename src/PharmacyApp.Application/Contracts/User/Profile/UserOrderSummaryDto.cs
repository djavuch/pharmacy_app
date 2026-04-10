using PharmacyApp.Application.Contracts.Order;
using PharmacyApp.Domain.Enums;

namespace PharmacyApp.Application.Contracts.User.Profile;

public record UserOrderSummaryDto
{
    public int OrderId { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public OrderStatus OrderStatus { get; set; }
    public int ItemsCount => OrderItems?.Count ?? 0;
    public List<OrderItemResponseDto> OrderItems { get; set; }
}