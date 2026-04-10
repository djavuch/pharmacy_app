using PharmacyApp.Application.Contracts.Wishlist;
using PharmacyApp.Domain.Entities;

namespace PharmacyApp.Application.Mappers;

public static class WishlistMappers
{
    public static WishlistDto ToWishlistDto(this Wishlist wishlist) => new()
    {
        ProductId = wishlist.ProductId,
        ProductName = wishlist.Product.Name
    };
}
