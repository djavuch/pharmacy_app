namespace PharmacyApp.Domain.Entities;

public class OrderAddressModel
{
    public int AddressId { get; set; } 
    public string Street { get; set; } = string.Empty;
    public string? ApartmentNumber { get; set; }
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string? AdditionalInfo { get; set; }
}
