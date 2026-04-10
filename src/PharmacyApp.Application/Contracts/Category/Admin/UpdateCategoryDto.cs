namespace PharmacyApp.Application.Contracts.Category.Admin;

public record UpdateCategoryDto
{
    public int CategoryId { get; set; } 
    public string CategoryName { get; set; }
    public string CategoryDescription { get; set; }
}
