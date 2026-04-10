namespace PharmacyApp.Application.Contracts.Discount;

public record CreateDiscountDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DiscountType { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public DateTime StartDate { get; set; } = DateTime.Now;
    public DateTime EndDate { get; set; } = DateTime.Now;
    public bool IsActive { get; set; }
    public decimal? MinimumOrderAmount { get; set; }
    public decimal? MaximumOrderAmount { get; set; }
    public List<int> ProductIds { get; set; } = [];
    public List<int> CategoryIds { get; set; } = [];
}
