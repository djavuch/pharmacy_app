using PharmacyApp.Application.Contracts.ShoppingCart;
using PharmacyApp.Domain.Entities;

namespace PharmacyApp.Application.Mappers;

public static class ShoppingCartMappers
{
    public static CartDto ToCartDto(
        this ShoppingCart cart,
        IReadOnlyDictionary<int, decimal>? effectivePrices = null) => new()
    {
        Id = cart.Id,
        UserId = cart.UserId,
        Items = cart.Items.Select(item => new CartItemDto
        {
            CartId = cart.Id,
            ProductId = item.ProductId,
            ProductName = item.Product?.Name ?? string.Empty,
            Quantity = item.Quantity,
            Price = ResolveEffectivePrice(item, effectivePrices),
            Subtotal = item.Quantity * ResolveEffectivePrice(item, effectivePrices),
            AvailableStock = item.Product?.StockQuantity ?? 0,
        }).ToList(),
        TotalPrice = cart.Items.Sum(item => item.Quantity * ResolveEffectivePrice(item, effectivePrices)),
        LastModifiedAt = cart.UpdatedAt ?? cart.CreatedAt
    };

    private static decimal ResolveEffectivePrice(CartItem item, IReadOnlyDictionary<int, decimal>? effectivePrices)
    {
        return effectivePrices is not null && effectivePrices.TryGetValue(item.ProductId, out var effectivePrice)
            ? effectivePrice
            : item.PriceAtAdd;
    }
}
