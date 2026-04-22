using PharmacyApp.Domain.Enums;

namespace PharmacyApp.Application.Contracts.Promotion;

public record PromotionListItemDto
{
    public Guid DiscountId { get; init; }
    public string Slug { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public DiscountType DiscountType { get; init; }
    public decimal Value { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public int ProductTargetsCount { get; init; }
    public int CategoryTargetsCount { get; init; }
}
