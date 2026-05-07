using PharmacyApp.Domain.Entities.PromoCode;
using PharmacyApp.Domain.Enums;

namespace PharmacyApp.UnitTests.Domain;

public sealed class PromoCodeTests
{
    [Theory]
    [InlineData("")]
    [InlineData("AB")]
    public void Constructor_WhenCodeIsInvalid_ThrowsArgumentException(string code)
    {
        Assert.Throws<ArgumentException>(() =>
            CreatePromoCode(code: code));
    }

    [Fact]
    public void Constructor_WhenPercentageValueExceedsOneHundred_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            CreatePromoCode(discountType: DiscountType.Percentage, value: 101m));
    }

    [Fact]
    public void Constructor_WhenEndDateIsBeforeStartDate_ThrowsArgumentException()
    {
        var startDate = DateTime.UtcNow;

        Assert.Throws<ArgumentException>(() =>
            CreatePromoCode(startDate: startDate, endDate: startDate.AddDays(-1)));
    }

    [Fact]
    public void IsValid_WhenPromoCodeIsActiveAndWithinDateRange_ReturnsTrue()
    {
        var promoCode = CreatePromoCode();

        Assert.True(promoCode.IsValid());
    }

    [Fact]
    public void IsValid_WhenPromoCodeIsInactive_ReturnsFalse()
    {
        var promoCode = CreatePromoCode();

        promoCode.Deactivate();

        Assert.False(promoCode.IsValid());
    }

    [Fact]
    public void IsValid_WhenMaxUsageCountReached_ReturnsFalse()
    {
        var promoCode = CreatePromoCode(maxUsageCount: 1);
        typeof(PromoCode)
            .GetProperty(nameof(PromoCode.CurrentUsageCount))!
            .SetValue(promoCode, 1);

        Assert.False(promoCode.IsValid());
    }

    [Fact]
    public void CanBeUsedByUser_WhenUserUsageLimitReached_ReturnsFalse()
    {
        var promoCode = CreatePromoCode(maxUsagePerUser: 1);
        promoCode.UsageHistory.Add(new PromoCodeUsage(promoCode.PromoCodeId, "user-1", 10, 5m));

        var canBeUsed = promoCode.CanBeUsedByUser("user-1");

        Assert.False(canBeUsed);
    }

    [Fact]
    public void CanBeUsedByUser_WhenDifferentUserUsedCode_ReturnsTrue()
    {
        var promoCode = CreatePromoCode(maxUsagePerUser: 1);
        promoCode.UsageHistory.Add(new PromoCodeUsage(promoCode.PromoCodeId, "user-1", 10, 5m));

        var canBeUsed = promoCode.CanBeUsedByUser("user-2");

        Assert.True(canBeUsed);
    }

    [Fact]
    public void UpdateApplicableProducts_WhenPromoAppliesToAllProducts_ClearsTargets()
    {
        var promoCode = CreatePromoCode(applicableToAllProducts: true);

        promoCode.UpdateApplicableProducts([1, 2], [3]);

        Assert.Empty(promoCode.PromoCodeProducts);
        Assert.Empty(promoCode.PromoCodeCategories);
    }

    [Fact]
    public void UpdateApplicableProducts_WhenPromoHasTargets_ReplacesTargets()
    {
        var promoCode = CreatePromoCode(applicableToAllProducts: false);

        promoCode.UpdateApplicableProducts([1, 2], [3]);

        Assert.Equal([1, 2], promoCode.PromoCodeProducts.Select(target => target.ProductId));
        Assert.Equal([3], promoCode.PromoCodeCategories.Select(target => target.CategoryId));
    }

    [Fact]
    public void Activate_WhenAlreadyActive_ThrowsInvalidOperationException()
    {
        var promoCode = CreatePromoCode();

        Assert.Throws<InvalidOperationException>(() => promoCode.Activate());
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_ThrowsInvalidOperationException()
    {
        var promoCode = CreatePromoCode();
        promoCode.Deactivate();

        Assert.Throws<InvalidOperationException>(() => promoCode.Deactivate());
    }

    private static PromoCode CreatePromoCode(
        string code = "SAVE10",
        DiscountType discountType = DiscountType.FixedAmount,
        decimal value = 10m,
        DateTime? startDate = null,
        DateTime? endDate = null,
        bool applicableToAllProducts = true,
        int? maxUsageCount = null,
        int? maxUsagePerUser = null)
    {
        var startsAt = startDate ?? DateTime.UtcNow.AddDays(-1);
        var endsAt = endDate ?? DateTime.UtcNow.AddDays(1);

        return new PromoCode(
            code,
            "Test promo code",
            discountType,
            value,
            startsAt,
            endsAt,
            applicableToAllProducts,
            maxUsageCount,
            maxUsagePerUser);
    }
}
