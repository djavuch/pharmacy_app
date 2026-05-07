using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using PharmacyApp.Domain.Enums;
using PharmacyApp.IntegrationTests.Support;

namespace PharmacyApp.IntegrationTests;

public sealed class CartAndOrdersApiTests : IAsyncLifetime
{
    private const string Password = "Password123";

    private readonly PharmacyAppWebApplicationFactory _factory = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync()
    {
        _factory.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task AddToCart_WhenProductExists_ReturnsCartWithItem()
    {
        var productId = await _factory.CreateProductAsync(stockQuantity: 5);
        await _factory.CreateConfirmedUserAsync(password: Password);

        var client = await CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/cart", new
        {
            productId,
            quantity = 2
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var cart = await response.Content.ReadFromJsonAsync<TestCartDto>();
        Assert.NotNull(cart);
        var item = Assert.Single(cart!.Items);

        Assert.Equal(productId, item.ProductId);
        Assert.Equal(2, item.Quantity);
        Assert.Equal(50m, cart.TotalPrice);
    }

    [Fact]
    public async Task AddToCart_WhenGuestAddsSameProductTwice_IncrementsExistingItem()
    {
        var productId = await _factory.CreateProductAsync(stockQuantity: 5);
        var client = _factory.CreateClient();

        var firstResponse = await client.PostAsJsonAsync("/cart", new
        {
            productId,
            quantity = 1
        });
        firstResponse.EnsureSuccessStatusCode();

        var secondResponse = await client.PostAsJsonAsync("/cart", new
        {
            productId,
            quantity = 1
        });

        Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);

        var cart = await secondResponse.Content.ReadFromJsonAsync<TestCartDto>();
        Assert.NotNull(cart);
        var item = Assert.Single(cart!.Items);

        Assert.Equal(productId, item.ProductId);
        Assert.Equal(2, item.Quantity);
        Assert.Equal(50m, cart.TotalPrice);
    }

    [Fact]
    public async Task AddToCart_WhenGuestUsesSessionHeaderWithoutCookies_IncrementsExistingItem()
    {
        var productId = await _factory.CreateProductAsync(stockQuantity: 5);
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = false
        });
        client.DefaultRequestHeaders.Add("X-Cart-Session-Id", Guid.NewGuid().ToString("N"));

        var firstResponse = await client.PostAsJsonAsync("/cart", new
        {
            productId,
            quantity = 1
        });
        firstResponse.EnsureSuccessStatusCode();

        var secondResponse = await client.PostAsJsonAsync("/cart", new
        {
            productId,
            quantity = 1
        });

        Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);

        var cart = await secondResponse.Content.ReadFromJsonAsync<TestCartDto>();
        Assert.NotNull(cart);
        var item = Assert.Single(cart!.Items);

        Assert.Equal(productId, item.ProductId);
        Assert.Equal(2, item.Quantity);
        Assert.Equal(50m, cart.TotalPrice);
    }

    [Fact]
    public async Task CreateOrder_WhenCartHasItems_CreatesOrderClearsCartAndDecreasesStock()
    {
        var productId = await _factory.CreateProductAsync(stockQuantity: 5);
        var user = await _factory.CreateConfirmedUserAsync(password: Password);

        var client = await CreateAuthenticatedClientAsync();
        var cartResponse = await client.PostAsJsonAsync("/cart", new
        {
            productId,
            quantity = 2
        });
        cartResponse.EnsureSuccessStatusCode();

        var orderResponse = await client.PostAsJsonAsync("/orders", new
        {
            newAddress = new
            {
                street = "Test Street 1",
                apartmentNumber = "10",
                city = "Kyiv",
                state = "Kyiv",
                zipCode = "01001",
                country = "Ukraine",
                additionalInfo = "Leave at reception"
            },
            saveAddress = false
        });

        Assert.Equal(HttpStatusCode.Created, orderResponse.StatusCode);

        var persisted = await _factory.ExecuteDbContextAsync(async dbContext =>
        {
            var product = await dbContext.Products.SingleAsync(product => product.Id == productId);
            var cartItemsCount = await dbContext.CartItems
                .Where(item => item.ShoppingCart.UserId == user.Id)
                .CountAsync();
            var order = await dbContext.Orders
                .Include(order => order.OrderItems)
                .SingleAsync(order => order.UserId == user.Id);

            return new
            {
                product.StockQuantity,
                CartItemsCount = cartItemsCount,
                OrderItemsCount = order.OrderItems.Count,
                order.TotalAmount
            };
        });

        Assert.Equal(3, persisted.StockQuantity);
        Assert.Equal(0, persisted.CartItemsCount);
        Assert.Equal(1, persisted.OrderItemsCount);
        Assert.Equal(50m, persisted.TotalAmount);
    }

    [Fact]
    public async Task CancelOrder_WhenOrderIsPending_ReturnsStockAndReversesEarnedBonuses()
    {
        var productId = await _factory.CreateProductAsync(stockQuantity: 5);
        var user = await _factory.CreateConfirmedUserAsync(password: Password);

        var client = await CreateAuthenticatedClientAsync();
        await AddProductToCartAsync(client, productId, 2);
        var orderId = await CreateOrderAsync(client);

        var cancelResponse = await client.PostAsync($"/orders/{orderId}/cancel", null);

        Assert.Equal(HttpStatusCode.NoContent, cancelResponse.StatusCode);

        var persisted = await _factory.ExecuteDbContextAsync(async dbContext =>
        {
            var product = await dbContext.Products.SingleAsync(product => product.Id == productId);
            var order = await dbContext.Orders.SingleAsync(order => order.Id == orderId);
            var bonusAccount = await dbContext.BonusAccounts.SingleAsync(account => account.UserId == user.Id);
            var bonusTransactionsCount = await dbContext.BonusTransactions.CountAsync(transaction => transaction.OrderId == orderId);

            return new
            {
                product.StockQuantity,
                order.OrderStatus,
                bonusAccount.Balance,
                BonusTransactionsCount = bonusTransactionsCount
            };
        });

        Assert.Equal(5, persisted.StockQuantity);
        Assert.Equal(OrderStatus.Cancelled, persisted.OrderStatus);
        Assert.Equal(0m, persisted.Balance);
        Assert.Equal(2, persisted.BonusTransactionsCount);
    }

    [Fact]
    public async Task CreateOrder_WhenPromoCodeIsValid_RecordsUsageAndAppliesDiscount()
    {
        var productId = await _factory.CreateProductAsync(stockQuantity: 5);
        var promoCodeId = await _factory.CreatePromoCodeAsync(code: "SAVE10", value: 10m);
        var user = await _factory.CreateConfirmedUserAsync(password: Password);

        var client = await CreateAuthenticatedClientAsync();
        await AddProductToCartAsync(client, productId, 2);

        var orderResponse = await client.PostAsJsonAsync("/orders", new
        {
            newAddress = CreateAddress(),
            saveAddress = false,
            promoCode = "SAVE10"
        });

        Assert.Equal(HttpStatusCode.Created, orderResponse.StatusCode);

        var persisted = await _factory.ExecuteDbContextAsync(async dbContext =>
        {
            var order = await dbContext.Orders.SingleAsync(order => order.UserId == user.Id);
            var promoCode = await dbContext.PromoCodes.SingleAsync(promoCode => promoCode.PromoCodeId == promoCodeId);
            var usage = await dbContext.PromoCodeUsages.SingleAsync(usage => usage.OrderId == order.Id);

            return new
            {
                order.TotalAmount,
                order.PromoCodeDiscountAmount,
                order.AppliedPromoCode,
                promoCode.CurrentUsageCount,
                usage.UserId,
                usage.DiscountApplied
            };
        });

        Assert.Equal(40m, persisted.TotalAmount);
        Assert.Equal(10m, persisted.PromoCodeDiscountAmount);
        Assert.Equal("SAVE10", persisted.AppliedPromoCode);
        Assert.Equal(1, persisted.CurrentUsageCount);
        Assert.Equal(user.Id, persisted.UserId);
        Assert.Equal(10m, persisted.DiscountApplied);
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/account/login", new
        {
            email = "customer@example.test",
            password = Password
        });

        response.EnsureSuccessStatusCode();
        var login = await response.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.False(string.IsNullOrWhiteSpace(login?.Token));

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login!.Token);
        return client;
    }

    private static async Task AddProductToCartAsync(HttpClient client, int productId, int quantity)
    {
        var response = await client.PostAsJsonAsync("/cart", new
        {
            productId,
            quantity
        });

        response.EnsureSuccessStatusCode();
    }

    private static async Task<int> CreateOrderAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/orders", new
        {
            newAddress = CreateAddress(),
            saveAddress = false
        });

        response.EnsureSuccessStatusCode();

        var order = await response.Content.ReadFromJsonAsync<TestOrderDto>();
        Assert.NotNull(order);

        return order!.Id;
    }

    private static object CreateAddress() => new
    {
        street = "Test Street 1",
        apartmentNumber = "10",
        city = "Kyiv",
        state = "Kyiv",
        zipCode = "01001",
        country = "Ukraine",
        additionalInfo = "Leave at reception"
    };

    private sealed record LoginResponse(string Token, string RefreshToken, string UserId);

    private sealed record TestCartDto(List<TestCartItemDto> Items, decimal TotalPrice);

    private sealed record TestCartItemDto(int ProductId, int Quantity);

    private sealed record TestOrderDto(int Id);
}
