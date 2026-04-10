using PharmacyApp.Domain.Enums;

namespace PharmacyApp.Domain.Entities;

public class Order
{
    public int Id { get; private set; }
    public string UserId { get; private set; } 
    public User User { get; set; }
    public DateTime OrderDate { get; private set; } = DateTime.Now.ToUniversalTime();

    public decimal TotalAmount { get; private set; }

    public OrderStatus OrderStatus { get; private set; }
    public int ShippingAddressId { get; private set; }
    public OrderAddress ShippingAddress { get; private set; }
    public List<OrderItem> OrderItems { get; set; } = [];

    public decimal SubtotalAmount { get; private set; }
    public decimal DiscountAmount { get; private set; } 
    public string? AppliedPromoCode { get;private set; } 

    public Guid? PromoCodeId { get; private set; } 
    public decimal PromoCodeDiscountAmount { get; private set; } = 0;

    // Bonus system properties
    public decimal BonusPointsRedeemed { get; private set; }
    public decimal BonusPointsEarned { get;private  set; }
    
    public byte[]? RowVersion { get; set; } = [];
    
    private Order() { }

    public Order(string userId, OrderAddress shippingAddress)
    {
        UserId = userId;
        OrderDate = DateTime.UtcNow;
        OrderStatus = OrderStatus.Pending;
        ShippingAddress = shippingAddress;
        OrderItems = new List<OrderItem>();
    }
    
    public void SetAmounts(decimal subtotal)
    {
        SubtotalAmount = subtotal;
        TotalAmount = subtotal;
        DiscountAmount = 0;
        PromoCodeDiscountAmount = 0;
    }

    public void ApplyPromoCode(string code, Guid promoCodeId, decimal discountAmount)
    {
        AppliedPromoCode = code.ToUpper();
        PromoCodeId = promoCodeId;
        PromoCodeDiscountAmount = discountAmount;
        DiscountAmount += discountAmount;
        TotalAmount = Math.Max(0, TotalAmount - discountAmount);
    }
    
    public void RemovePromoCode()
    {
        AppliedPromoCode = null;
        PromoCodeId = null;
        PromoCodeDiscountAmount = 0;
    }
    
    public void ApplyBonusRedemption(decimal pointsRedeemed, decimal bonusDiscount)
    {
        BonusPointsRedeemed = pointsRedeemed;
        DiscountAmount += bonusDiscount;
        TotalAmount = Math.Max(0, TotalAmount - bonusDiscount);
    }
    
    public void SetBonusPointsEarned(decimal pointsEarned)
    {
        BonusPointsEarned = pointsEarned;
    }
    
    public void ChangeStatus(OrderStatus status)
    {
        OrderStatus = status;
    }
    
    public void UpdateShippingAddress(OrderAddress address)
    {
        ShippingAddress = address;
        ShippingAddressId = address.AddressId;
    }
}
