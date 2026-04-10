using PharmacyApp.Application.Contracts.Order;
using PharmacyApp.Domain.Entities;

namespace PharmacyApp.Application.Mappers;

public static class OrderMappers
{
    public static OrderDetailsDto ToOrderResponseDto(this Order order) => new()
    {
        Id = order.Id,
        BuyerFirstName = order.User.FirstName,
        BuyerLastName = order.User.LastName,
        OrderDate = order.OrderDate,
        TotalAmount = order.TotalAmount,
        OrderStatus = order.OrderStatus,
        OrderItems = order.OrderItems.Select(oi => new OrderItemResponseDto
        {
            OrderId = oi.OrderId,
            ProductName = oi.ProductName,
            Quantity = oi.Quantity,
            Price = oi.Price
        }).ToList(),
        ShippingAddress = order.ShippingAddress?.ToOrderAddressDto(),
        AppliedPromoCode = order.AppliedPromoCode,
        PromoCodeId = order.PromoCodeId,
        PromoCodeDiscountAmount = order.PromoCodeDiscountAmount,
        BonusPointsRedeemed = order.BonusPointsRedeemed,
        BonusPointsEarned = order.BonusPointsEarned
    };

    public static OrderAddressDto ToOrderAddressDto(this OrderAddress address) => new()
    {
        Street = address.Street,
        City = address.City,
        State = address.State,
        ZipCode = address.ZipCode,
        Country = address.Country,
        ApartmentNumber = address.ApartmentNumber,
        AdditionalInfo = address.AdditionalInfo
    };
}
