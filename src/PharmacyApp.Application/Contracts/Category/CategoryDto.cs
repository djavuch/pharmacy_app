namespace PharmacyApp.Application.Contracts.Category;

public record CategoryDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; }
    public string CategoryDescription { get; set; }
}
