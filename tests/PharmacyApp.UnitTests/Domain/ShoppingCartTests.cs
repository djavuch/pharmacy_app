using PharmacyApp.Domain.Entities;

namespace PharmacyApp.UnitTests.Domain;

public sealed class ShoppingCartTests
{
    [Fact]
    public void Constructor_WhenUserIdAndSessionIdAreEmpty_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new ShoppingCart("", null));
    }

    [Fact]
    public void GetTotalAmount_WhenCartHasItems_ReturnsQuantityMultipliedByPrices()
    {
        var cart = new ShoppingCart(null!, "guest-session");
        cart.Items.Add(new CartItem(cartId: 1, productId: 10, quantity: 2, priceAtAdd: 25m));
        cart.Items.Add(new CartItem(cartId: 1, productId: 11, quantity: 3, priceAtAdd: 10m));

        var total = cart.GetTotalAmount();

        Assert.Equal(80m, total);
    }

    [Fact]
    public void AssignToUser_WhenCalled_SetsUserAndClearsSession()
    {
        var cart = new ShoppingCart(null!, "guest-session");

        cart.AssignToUser("user-1");

        Assert.Equal("user-1", cart.UserId);
        Assert.Null(cart.SessionId);
        Assert.NotNull(cart.UpdatedAt);
    }

    [Fact]
    public void AssignToGuest_WhenCalled_SetsSessionAndUpdatesTimestamp()
    {
        var cart = new ShoppingCart("user-1", null);

        cart.AssignToGuest("guest-session");

        Assert.Equal("user-1", cart.UserId);
        Assert.Equal("guest-session", cart.SessionId);
        Assert.NotNull(cart.UpdatedAt);
    }
}
