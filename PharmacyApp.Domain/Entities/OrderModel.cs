using PharmacyApp.Domain.Enums;

namespace PharmacyApp.Domain.Entities;

public class OrderModel
{
    public int Id { get; set; }
    public string UserId { get; set; } 
    public UserModel User { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.Now.ToUniversalTime();

    public decimal TotalAmount { get; set; } // Final amount after applying discounts and taxes

    public OrderStatus OrderStatus { get; set; }
    public int ShippingAddressId { get; set; }
    public OrderAddressModel ShippingAddress { get; set; }
    public List<OrderItemModel> OrderItems { get; set; } = [];

    public decimal SubtotalAmount { get; set; } // Total before discounts and taxes
    public decimal DiscountAmount { get; set; } // Total discount applied to the order
    public string? AppliedPromoCode { get; set; } // Promo code used for the order, if any

    public Guid? PromoCodeId { get; set; } 
    public decimal PromoCodeDiscountAmount { get; set; } = 0;

    // Bonus system properties
    public decimal BonusPointsRedeemed { get; set; }
    public decimal BonusPointsEarned { get; set; }
}
