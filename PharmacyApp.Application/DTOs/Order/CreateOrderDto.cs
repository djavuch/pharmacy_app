namespace PharmacyApp.Application.DTOs.Order;

public class CreateOrderDto
{
    public int? SavedAddressId { get; set; }
    public OrderAddressDto? NewAddress { get; set; }

    // Save new address option
    public bool SaveAddress { get; set; }
    public string? SavedLabel { get; set; }

    public string? PromoCode { get; set; }
    public decimal? RedeemBonusPoints { get; set; } 
}
