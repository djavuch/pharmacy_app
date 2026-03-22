namespace PharmacyApp.Application.DTOs.PromoCode;

public record PromoCodeValidationResultDto
{
    public bool IsValid { get; set; }
    public string Message { get; set; } = string.Empty;
    public decimal DiscountAmount { get; set; }
    public Guid? PromoCodeId { get; set; }
}