namespace PharmacyApp.Application.DTOs.Admin.ProductCategory;

public record CategoryDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; }
    public string CategoryDescription { get; set; }
}
