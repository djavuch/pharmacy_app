using PharmacyApp.Domain.Entities;
using PharmacyApp.Domain.Enums;

namespace PharmacyApp.Application.DTOs.Order;

public record OrderResponseDto
{
    public int Id { get; set; }
    public string BuyerFirstName { get; set; }
    public string BuyerLastName { get; set; }
    public string BuyerFullName => $"{BuyerFirstName} {BuyerLastName}";
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public OrderStatus OrderStatus { get; set; }
    public OrderAddressDto? ShippingAddress { get; set; }
    public List<OrderItemResponseDto> OrderItems { get; set; }

    public string? AppliedPromoCode { get; set; }
    public Guid? PromoCodeId { get; set; }
    public decimal PromoCodeDiscountAmount { get; set; }
    public decimal BonusPointsRedeemed { get; set; }
    public decimal BonusPointsEarned { get; set; }
}