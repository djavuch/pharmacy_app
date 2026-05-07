using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using PharmacyApp.IntegrationTests.Support;

namespace PharmacyApp.IntegrationTests;

public sealed class AccountApiTests : IAsyncLifetime
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
    public async Task RefreshToken_WhenTokenIsActive_RotatesStoredToken()
    {
        await _factory.CreateConfirmedUserAsync(password: Password);
        var client = _factory.CreateClient();

        var login = await LoginAsync(client);
        var oldRefreshToken = login.RefreshToken;

        var refreshResponse = await client.PostAsJsonAsync("/account/refresh-token", new
        {
            refreshToken = oldRefreshToken
        });

        Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);

        var refreshed = await refreshResponse.Content.ReadFromJsonAsync<RefreshTokenResponse>();
        Assert.NotNull(refreshed);
        Assert.False(string.IsNullOrWhiteSpace(refreshed!.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(refreshed.RefreshToken));
        Assert.NotEqual(oldRefreshToken, refreshed.RefreshToken);

        var persisted = await _factory.ExecuteDbContextAsync(async dbContext =>
        {
            var oldToken = await dbContext.RefreshTokens.SingleAsync(token => token.Token == oldRefreshToken);
            var newToken = await dbContext.RefreshTokens.SingleAsync(token => token.Token == refreshed.RefreshToken);

            return new
            {
                OldTokenRevoked = oldToken.IsRevoked,
                NewTokenRevoked = newToken.IsRevoked
            };
        });

        Assert.True(persisted.OldTokenRevoked);
        Assert.False(persisted.NewTokenRevoked);
    }

    [Fact]
    public async Task AdminProducts_WhenUserIsCustomer_ReturnsForbidden()
    {
        await _factory.CreateConfirmedUserAsync(password: Password);
        var client = _factory.CreateClient();
        var login = await LoginAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.Token);

        var response = await client.GetAsync("/admin/products");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private static async Task<LoginResponse> LoginAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/account/login", new
        {
            email = "customer@example.test",
            password = Password
        });

        response.EnsureSuccessStatusCode();

        var login = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(login);

        return login!;
    }

    private sealed record LoginResponse(string Token, string RefreshToken, string UserId);

    private sealed record RefreshTokenResponse(string AccessToken, string RefreshToken, string Message);
}
