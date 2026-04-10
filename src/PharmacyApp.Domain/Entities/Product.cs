using PharmacyApp.Domain.Entities.Discount;
using System.ComponentModel.DataAnnotations;

namespace PharmacyApp.Domain.Entities;

public class Product
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int StockQuantity { get; private set; }
    public string ImageUrl { get; private set; } = string.Empty;
    public int CategoryId { get; private set; }
    public int WishlistCount { get; private set; }
    
    public Category Category { get; set; } 
    public ICollection<Review> Reviews { get; set; } = [];
    public ICollection<Wishlist> Wishlist { get; set; } = [];
    public ICollection<ProductDiscount> ProductDiscounts { get; set; } = [];
    
     

    [Timestamp]
    public byte[]? RowVersion { get; set; }
    
    private Product() { }

    public Product(string name, string description, 
        decimal price, int stockQuantity, string imageUrl, Category category)
    {
        Name = name;
        Description = description;
        Price = price;
        StockQuantity = stockQuantity;
        ImageUrl = imageUrl;
        Category = category;
        CategoryId = category.CategoryId;
        ValidateBusinessRules();
    }
    
    public void Update(string name, string description,
        decimal price, int stockQuantity, string imageUrl, Category category)
    {
        Name = name;
        Description = description;
        Price = price;
        StockQuantity = stockQuantity;
        ImageUrl = imageUrl;
        Category = category;
        CategoryId = category.CategoryId;
        ValidateBusinessRules(); 
    }

    public void UpdateStockQuantity(int quantityChange)
    {
        if (StockQuantity + quantityChange < 0)
            throw new ArgumentException($"Insufficient stock. Available: {StockQuantity}, requested change: {quantityChange}.");

        StockQuantity += quantityChange;
    }

    private void ValidateBusinessRules()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new ArgumentException("Product name cannot be empty.");
        if (Price <= 0)
            throw new ArgumentException("Product price cannot be negative or 0.");
        if (StockQuantity < 0)
            throw new ArgumentException("Stock quantity cannot be negative.");
    }
}
