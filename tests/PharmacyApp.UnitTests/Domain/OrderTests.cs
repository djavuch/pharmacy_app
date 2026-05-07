using PharmacyApp.Domain.Entities;
using PharmacyApp.Domain.Enums;

namespace PharmacyApp.UnitTests.Domain;

public sealed class OrderTests
{
    [Fact]
    public void Constructor_WhenCalled_CreatesPendingOrder()
    {
        var address = CreateAddress();

        var order = new Order("user-1", address);

        Assert.Equal("user-1", order.UserId);
        Assert.Equal(OrderStatus.Pending, order.OrderStatus);
        Assert.Same(address, order.ShippingAddress);
        Assert.Empty(order.OrderItems);
    }

    [Fact]
    public void SetAmounts_WhenCalled_ResetsDiscountsAndTotal()
    {
        var order = CreateOrder();

        order.SetAmounts(100m);
        order.ApplyPromoCode("save10", Guid.NewGuid(), 10m);
        order.SetAmounts(80m);

        Assert.Equal(80m, order.SubtotalAmount);
        Assert.Equal(80m, order.TotalAmount);
        Assert.Equal(0m, order.DiscountAmount);
        Assert.Equal(0m, order.PromoCodeDiscountAmount);
    }

    [Fact]
    public void ApplyPromoCode_WhenDiscountIsLessThanTotal_UpdatesAmountsAndCode()
    {
        var promoCodeId = Guid.NewGuid();
        var order = CreateOrder();
        order.SetAmounts(100m);

        order.ApplyPromoCode("save10", promoCodeId, 15m);

        Assert.Equal("SAVE10", order.AppliedPromoCode);
        Assert.Equal(promoCodeId, order.PromoCodeId);
        Assert.Equal(15m, order.PromoCodeDiscountAmount);
        Assert.Equal(15m, order.DiscountAmount);
        Assert.Equal(85m, order.TotalAmount);
    }

    [Fact]
    public void ApplyPromoCode_WhenDiscountExceedsTotal_DoesNotMakeTotalNegative()
    {
        var order = CreateOrder();
        order.SetAmounts(10m);

        order.ApplyPromoCode("free", Guid.NewGuid(), 20m);

        Assert.Equal(0m, order.TotalAmount);
    }

    [Fact]
    public void ApplyBonusRedemption_WhenDiscountExceedsTotal_DoesNotMakeTotalNegative()
    {
        var order = CreateOrder();
        order.SetAmounts(10m);

        order.ApplyBonusRedemption(pointsRedeemed: 20m, bonusDiscount: 20m);

        Assert.Equal(20m, order.BonusPointsRedeemed);
        Assert.Equal(20m, order.DiscountAmount);
        Assert.Equal(0m, order.TotalAmount);
    }

    [Fact]
    public void ChangeStatus_WhenCalled_UpdatesStatus()
    {
        var order = CreateOrder();

        order.ChangeStatus(OrderStatus.Processing);

        Assert.Equal(OrderStatus.Processing, order.OrderStatus);
    }

    [Theory]
    [InlineData(0, "Aspirin", 1, 10)]
    [InlineData(1, "", 1, 10)]
    [InlineData(1, "Aspirin", 0, 10)]
    [InlineData(1, "Aspirin", 1, 0)]
    public void OrderItemConstructor_WhenRequiredValueIsInvalid_ThrowsArgumentException(
        int productId,
        string productName,
        int quantity,
        decimal price)
    {
        Assert.Throws<ArgumentException>(() =>
            new OrderItem(productId, productName, quantity, price));
    }

    [Fact]
    public void OrderItemTotal_WhenNoDiscount_ReturnsSubtotal()
    {
        var item = new OrderItem(productId: 1, productName: "Aspirin", quantity: 3, price: 10m);

        Assert.Equal(30m, item.Subtotal);
        Assert.Equal(30m, item.Total);
    }

    [Theory]
    [InlineData("", "City", "State", "01001", "Ukraine")]
    [InlineData("Street", "", "State", "01001", "Ukraine")]
    [InlineData("Street", "City", "", "01001", "Ukraine")]
    [InlineData("Street", "City", "State", "", "Ukraine")]
    [InlineData("Street", "City", "State", "01001", "")]
    public void OrderAddressConstructor_WhenRequiredValueIsEmpty_ThrowsArgumentException(
        string street,
        string city,
        string state,
        string zipCode,
        string country)
    {
        Assert.Throws<ArgumentException>(() =>
            new OrderAddress(street, null, city, state, zipCode, country));
    }

    private static Order CreateOrder() => new("user-1", CreateAddress());

    private static OrderAddress CreateAddress() =>
        new("Test Street", "10", "Kyiv", "Kyiv", "01001", "Ukraine");
}
