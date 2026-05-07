using PharmacyApp.Domain.Entities.Discount;
using PharmacyApp.Domain.Enums;

namespace PharmacyApp.UnitTests.Domain;

public sealed class DiscountTests
{
    [Fact]
    public void Constructor_WhenNameIsEmpty_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            CreateDiscount(name: ""));
    }

    [Fact]
    public void Constructor_WhenValueIsZero_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            CreateDiscount(value: 0m));
    }

    [Fact]
    public void Constructor_WhenPercentageValueExceedsOneHundred_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            CreateDiscount(discountType: DiscountType.Percentage, value: 101m));
    }

    [Fact]
    public void Constructor_WhenEndDateIsBeforeStartDate_ThrowsArgumentException()
    {
        var startDate = DateTime.UtcNow;

        Assert.Throws<ArgumentException>(() =>
            CreateDiscount(startDate: startDate, endDate: startDate.AddDays(-1)));
    }

    [Fact]
    public void IsValid_WhenActiveAndWithinDateRange_ReturnsTrue()
    {
        var discount = CreateDiscount();

        Assert.True(discount.isValid());
    }

    [Fact]
    public void IsValid_WhenInactive_ReturnsFalse()
    {
        var discount = CreateDiscount(isActive: false);

        Assert.False(discount.isValid());
    }

    [Fact]
    public void Update_WhenValuesAreValid_ReplacesDiscountData()
    {
        var discount = CreateDiscount();
        var startsAt = DateTime.UtcNow.AddDays(-2);
        var endsAt = DateTime.UtcNow.AddDays(2);

        discount.Update(
            "Updated",
            "Updated description",
            DiscountType.Percentage,
            15m,
            startsAt,
            endsAt,
            minimumOrderAmount: 50m,
            maximumOrderAmount: 500m,
            isActive: false);

        Assert.Equal("Updated", discount.Name);
        Assert.Equal("Updated description", discount.Description);
        Assert.Equal(DiscountType.Percentage, discount.DiscountType);
        Assert.Equal(15m, discount.Value);
        Assert.Equal(50m, discount.MinimumOrderAmount);
        Assert.Equal(500m, discount.MaximumOrderAmount);
        Assert.False(discount.IsActive);
    }

    private static Discount CreateDiscount(
        string name = "Sale",
        DiscountType discountType = DiscountType.FixedAmount,
        decimal value = 10m,
        DateTime? startDate = null,
        DateTime? endDate = null,
        bool isActive = true)
    {
        return new Discount(
            name,
            "Sale description",
            discountType,
            value,
            startDate ?? DateTime.UtcNow.AddDays(-1),
            endDate ?? DateTime.UtcNow.AddDays(1),
            isActive: isActive);
    }
}
