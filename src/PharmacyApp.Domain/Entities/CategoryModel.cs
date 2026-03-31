using PharmacyApp.Domain.Entities.Discount;
using System.ComponentModel.DataAnnotations;

namespace PharmacyApp.Domain.Entities;

public class CategoryModel
{
    public int CategoryId { get; set; }
    [Required]
    public string CategoryName { get; set; }
    [Required]
    public string CategoryDescription { get; set; }
    public ICollection<ProductModel> Products { get; set; } 
    public ICollection<CategoryDiscountModel> CategoryDiscounts { get; set; }
}
