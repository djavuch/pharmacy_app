namespace PharmacyApp.Application.DTOs.PromoCode;

public class ValidatePromoCodeDto
{
    public string Code { get; set; } = string.Empty;
    public string UserId { get; set; }
    public decimal OrderAmount { get; set; }
    public List<int> ProductIds { get; set; } = [];
    public List<int> CategoryIds { get; set; } = [];
}