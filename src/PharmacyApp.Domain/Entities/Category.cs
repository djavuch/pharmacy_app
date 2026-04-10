using PharmacyApp.Domain.Entities.Discount;
using System.ComponentModel.DataAnnotations;

namespace PharmacyApp.Domain.Entities;

public class Category
{
    public int CategoryId { get; private set; }
    public string CategoryName { get; private set; } = string.Empty;
    public string CategoryDescription { get; private set; } = string.Empty;

    public ICollection<Product> Products { get; set; } = [];
    public ICollection<CategoryDiscount> CategoryDiscounts { get; set; } = [];
    
    private Category() {}
    
    public Category(string categoryName, string categoryDescription)
    {
        CategoryName = categoryName;
        CategoryDescription = categoryDescription;
    }
    
    public void Update(string categoryName, string categoryDescription)
    {
        CategoryName = categoryName;
        CategoryDescription = categoryDescription;
    }
}
