using PharmacyApp.Domain.Entities;
using PharmacyApp.Domain.Enums;

namespace PharmacyApp.Application.DTOs.Order;

public record UpdateOrderStatusDto
{
    public int OrderId { get; set; }
    public OrderStatus Status { get; set; }
}
