using PharmacyApp.Application.Contracts.Product;
using PharmacyApp.Domain.Entities;

namespace PharmacyApp.Application.Mappers;

public static class ProductMappers
{
    public static ProductDto ToProductDto(
        this Product product,
        decimal? discountedPrice = null,
        int reviewCount = 0,
        decimal averageRating = 0m)
    {
        return new ProductDto
        {
            Id = product.Id,
            ProductCode = !string.IsNullOrWhiteSpace(product.ProductCode)
                ? product.ProductCode
                : FormatProductCode(product.Id),
            Name = product.Name,
            Description = product.Description,
            ImageUrl = product.ImageUrl,
            Price = product.Price,
            DiscountedPrice = discountedPrice,
            StockQuantity = product.StockQuantity,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.CategoryName ?? string.Empty,
            WishlistCount = product.WishlistCount,
            ReviewCount = reviewCount,
            AverageRating = averageRating
        };
    }

    private static string FormatProductCode(int productId) => $"PRD-{productId:D6}";
}
