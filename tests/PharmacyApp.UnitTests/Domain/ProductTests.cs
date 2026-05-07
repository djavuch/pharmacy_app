using PharmacyApp.Domain.Entities;

namespace PharmacyApp.UnitTests.Domain;

public sealed class ProductTests
{
    [Fact]
    public void Constructor_WhenNameIsEmpty_ThrowsArgumentException()
    {
        var category = new Category("Pain relief", "Pain relief products");

        Assert.Throws<ArgumentException>(() =>
            new Product("", "Description", 10m, 5, "/image.png", category));
    }

    [Fact]
    public void Constructor_WhenPriceIsZero_ThrowsArgumentException()
    {
        var category = new Category("Pain relief", "Pain relief products");

        Assert.Throws<ArgumentException>(() =>
            new Product("Aspirin", "Description", 0m, 5, "/image.png", category));
    }

    [Fact]
    public void Constructor_WhenStockIsNegative_ThrowsArgumentException()
    {
        var category = new Category("Pain relief", "Pain relief products");

        Assert.Throws<ArgumentException>(() =>
            new Product("Aspirin", "Description", 10m, -1, "/image.png", category));
    }

    [Fact]
    public void UpdateStockQuantity_WhenResultWouldBeNegative_ThrowsArgumentException()
    {
        var product = CreateProduct(stockQuantity: 2);

        Assert.Throws<ArgumentException>(() => product.UpdateStockQuantity(-3));
    }

    [Fact]
    public void UpdateStockQuantity_WhenChangeIsValid_UpdatesStock()
    {
        var product = CreateProduct(stockQuantity: 2);

        product.UpdateStockQuantity(3);

        Assert.Equal(5, product.StockQuantity);
    }

    [Fact]
    public void AssignProductCode_WhenCodeHasWhitespace_NormalizesCode()
    {
        var product = CreateProduct();

        product.AssignProductCode(" prd-001 ");

        Assert.Equal("PRD-001", product.ProductCode);
    }

    private static Product CreateProduct(int stockQuantity = 5)
    {
        var category = new Category("Pain relief", "Pain relief products");
        return new Product("Aspirin", "Description", 10m, stockQuantity, "/image.png", category);
    }
}
