namespace PharmacyApp.Application.Contracts.PromoCode.Results;

public record PromoCodeValidationResults
{
    public string Code { get; set; } = string.Empty;
    public string UserId { get; set; }
    public decimal OrderAmount { get; set; }
    public List<int> ProductIds { get; set; } = [];
    public List<int> CategoryIds { get; set; } = [];
}