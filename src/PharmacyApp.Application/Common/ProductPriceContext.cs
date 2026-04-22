namespace PharmacyApp.Application.Common;

public readonly record struct ProductPriceContext(
    int ProductId,
    int CategoryId,
    decimal OriginalPrice
);