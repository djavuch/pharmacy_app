using PharmacyApp.Application.Contracts.Product;
using PharmacyApp.Domain.Entities;

namespace PharmacyApp.Application.Mappers;

public static class ProductMappers
{
    public static ProductDto ToProductDto(this Product product, decimal? discountedPrice = null) => new()
    {
        Id = product.Id,
        Name = product.Name,
        Description = product.Description,
        Price = product.Price,
        DiscountedPrice = discountedPrice,
        StockQuantity = product.StockQuantity,
        CategoryId = product.CategoryId,
        CategoryName = product.Category?.CategoryName ?? string.Empty,
        WishlistCount = product.WishlistCount
    };
}
