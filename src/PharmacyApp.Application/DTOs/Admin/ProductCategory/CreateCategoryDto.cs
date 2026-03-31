namespace PharmacyApp.Application.DTOs.Admin.ProductCategory;

public record CreateCategoryDto
{
    public string CategoryName { get; set; }
    public string CategoryDescription { get; set; }
}
