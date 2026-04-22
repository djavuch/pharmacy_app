namespace PharmacyApp.Application.Contracts.Product;
public  record ProductDto
{
    public int Id { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountedPrice { get; set; }
    public int StockQuantity { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int WishlistCount { get; set; }
    public decimal AverageRating { get; set; }
    public int ReviewCount { get; set; }
}
