using PharmacyApp.Domain.Enums;

namespace PharmacyApp.Application.Contracts.Order;

public record UpdateOrderStatusDto
{
    public int OrderId { get; set; }
    public OrderStatus Status { get; set; }
}
