using PharmacyApp.Domain.Enums;

namespace PharmacyApp.Application.Contracts.Order;

public sealed record OrderCancelDto(
    int Id,
    string UserId,
    OrderStatus OrderStatus,
    Guid? PromoCodeId
);

public sealed record OrderItemStockDto(int ProductId, int Quantity);