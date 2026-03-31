using PharmacyApp.Domain.Entities.Discount;
using System.ComponentModel.DataAnnotations;

namespace PharmacyApp.Domain.Entities;

public class ProductModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public CategoryModel Category { get; set; } 
    public List<ReviewModel> Reviews { get; set; } = [];
    public ICollection<WishlistModel> Wishlist { get; set; } = [];
    public ICollection<ProductDiscountModel> ProductDiscounts { get; set; } = [];
    
    public int WishlistCount { get; set; } = 0;

    [Timestamp]
    public byte[]? RowVersion { get; set; } // race condition handling

    public void ValidateBusinessRules()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new ArgumentException("Product name cannot be empty.");

        if (Price <= 0)
            throw new ArgumentException("Product price cannot be negative or 0.");

        if (StockQuantity < 0)
            throw new ArgumentException("Stock quantity cannot be negative.");
    }
}
