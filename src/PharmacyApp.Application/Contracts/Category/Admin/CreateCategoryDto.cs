namespace PharmacyApp.Application.Contracts.Category.Admin;

public record CreateCategoryDto
{
    public string CategoryName { get; set; }
    public string CategoryDescription { get; set; }
}
