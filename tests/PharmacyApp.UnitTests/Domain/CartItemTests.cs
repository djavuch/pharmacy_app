using PharmacyApp.Domain.Entities;

namespace PharmacyApp.UnitTests.Domain;

public sealed class CartItemTests
{
    [Theory]
    [InlineData(0, 1, 1, 10)]
    [InlineData(1, 0, 1, 10)]
    [InlineData(1, 1, 0, 10)]
    [InlineData(1, 1, 1, 0)]
    public void Constructor_WhenRequiredValueIsInvalid_ThrowsArgumentException(
        int cartId,
        int productId,
        int quantity,
        decimal priceAtAdd)
    {
        Assert.Throws<ArgumentException>(() =>
            new CartItem(cartId, productId, quantity, priceAtAdd));
    }

    [Fact]
    public void AddQuantity_WhenQuantityIsPositive_IncreasesQuantity()
    {
        var item = new CartItem(cartId: 1, productId: 10, quantity: 1, priceAtAdd: 25m);

        item.AddQuantity(2);

        Assert.Equal(3, item.Quantity);
    }

    [Fact]
    public void AddQuantity_WhenQuantityIsZero_ThrowsArgumentException()
    {
        var item = new CartItem(cartId: 1, productId: 10, quantity: 1, priceAtAdd: 25m);

        Assert.Throws<ArgumentException>(() => item.AddQuantity(0));
    }

    [Fact]
    public void SetQuantity_WhenQuantityIsPositive_ReplacesQuantity()
    {
        var item = new CartItem(cartId: 1, productId: 10, quantity: 1, priceAtAdd: 25m);

        item.SetQuantity(5);

        Assert.Equal(5, item.Quantity);
    }

    [Fact]
    public void SetQuantity_WhenQuantityIsZero_ThrowsArgumentException()
    {
        var item = new CartItem(cartId: 1, productId: 10, quantity: 1, priceAtAdd: 25m);

        Assert.Throws<ArgumentException>(() => item.SetQuantity(0));
    }
}
