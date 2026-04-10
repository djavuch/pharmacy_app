using PharmacyApp.Application.Contracts.ShoppingCart;
using PharmacyApp.Domain.Entities;

namespace PharmacyApp.Application.Mappers;

public static class ShoppingCartMappers
{
    public static CartDto ToCartDto(this ShoppingCart cart) => new()
    {
        Id = cart.Id,
        UserId = cart.UserId,
        Items = cart.Items.Select(item => new CartItemDto
        {
            CartId = cart.Id,
            ProductId = item.ProductId,
            ProductName = item.Product.Name,
            Quantity = item.Quantity,
            Price = item.PriceAtAdd,
            Subtotal = item.Quantity * item.PriceAtAdd,
            AvailableStock = item.Product.StockQuantity,
        }).ToList(),
        TotalPrice = cart.GetTotalAmount(),
        LastModifiedAt = cart.UpdatedAt ?? cart.CreatedAt
    };
}
