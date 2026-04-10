using PharmacyApp.Domain.Enums;

namespace PharmacyApp.Application.Contracts.Discount;

public record DiscountDto
{
    public Guid DiscountId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DiscountType DiscountType { get; set; } 
    public decimal Value { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public decimal? MinimumOrderAmount { get; set; }
    public decimal? MaximumOrderAmount { get; set; }
    public List<int> ProductIds { get; set; } = [];
    public List<int> CategoryIds { get; set; } = [];
}
