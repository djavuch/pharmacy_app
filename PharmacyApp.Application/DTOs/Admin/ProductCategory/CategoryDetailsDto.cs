using PharmacyApp.Application.DTOs.Product;

namespace PharmacyApp.Application.DTOs.Admin.ProductCategory;

public class CategoryDetailsDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; }
    public string CategoryDescription { get; set; }
    public ICollection<ProductDto> Products { get; set; }
}