using System.Net;
using System.Text.Json;
using PharmacyApp.IntegrationTests.Support;

namespace PharmacyApp.IntegrationTests;

public sealed class StoreApiTests : IAsyncLifetime
{
    private readonly PharmacyAppWebApplicationFactory _factory = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync()
    {
        _factory.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetProducts_ReturnsSeededProduct()
    {
        var productId = await _factory.CreateProductAsync(name: "Integration Aspirin", price: 12.50m);
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/store/products?filterOn=name&filterQuery=Integration%20Aspirin");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(payload.RootElement.GetProperty("success").GetBoolean());

        var items = payload.RootElement.GetProperty("data").GetProperty("items");
        var item = Assert.Single(items.EnumerateArray());

        Assert.Equal(productId, item.GetProperty("id").GetInt32());
        Assert.Equal("Integration Aspirin", item.GetProperty("name").GetString());
        Assert.Equal(12.50m, item.GetProperty("price").GetDecimal());
    }
}
